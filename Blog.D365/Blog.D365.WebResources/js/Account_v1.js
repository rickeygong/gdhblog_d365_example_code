/**
 * Account Entity Javascript.
 */
if (Gdh === undefined) { var Gdh = {}; }
if (Gdh.D365 === undefined) { Gdh.D365 = {}; }
Gdh.D365.Account = (function () {
    'use strict';
    return {
        Constants: {
            Fields: {
                AccountName: "name",
                Phone: "telephone1",
                Fax: "fax",
                Website: "websiteurl",
            },
            Reports: {
                PrintAccountReport: "PrintAccount.rdl",
            },
            SystemAdminId: "SystemAdminId",
        },
        OnLoad: function (ExecutionContext) {
            try {
                let objFormContext = ExecutionContext.getFormContext();

            } catch (e) {
                console.error("Error during OnLoad: ", e);
            }
        },
        ExportPrintAccountReportPDF: function (primaryControl) {
            let objFormContext = primaryControl;
            let CurrentAccountId = objFormContext.data.entity.getId().replace("{", "").replace("}", "");
            let that = this;
            console.log(CurrentAccountId, CurrentAccountId);
            let selectAttributes = `${that.Constants.Fields.AccountName}`;
            console.log(selectAttributes, selectAttributes);
            let accountEn = this.RetrieveSingleRecord("accounts", CurrentAccountId, selectAttributes);
            let accountName = accountEn[this.Constants.Fields.AccountName];
            // CRM_FilteredAccount -> SSRS report argument
            let reportPrefilter = "CRM_FilteredAccount=" + "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                "<entity name='account'>" +
                "  <all-attributes />" +
                "  <filter type='and'>" +
                "    <condition attribute='accountid' operator='eq' value='" + CurrentAccountId + "' />" +
                "  </filter>" +
                "</entity>" +
                "</fetch>";
            let arrReportSession = this.ExecuteReport(this.Constants.Reports.PrintAccountReport, reportPrefilter);
            this.Get_SSRS_Report_PDFBase64(arrReportSession, 2052).then(function (base64String) {
                // Size of the file in KB
                let fSize = (encodeURIComponent(base64String).replace(/%../g, 'x').length) / 1024;
                let openFileOptions = { openMode: 2 };
                let file = {};
                file.fileContent = base64String;
                file.fileSize = fSize;
                // Set file name
                file.fileName = accountName + " - Info" + ".pdf";
                file.mimeType = "application/pdf";
                Xrm.Navigation.openFile(file, openFileOptions);
            }).catch(function (error) {
                console.error(error);
            });
        },
        GetReportIdByReportFileName: function (reportFileName) {
            let lValue = "";
            let lResponse = this.RetrieveMultipleRecord("reports", "filename eq '" + reportFileName + "'", "reportid", false);
            if (lResponse !== null && lResponse !== undefined && lResponse.value.length > 0) {
                lValue = lResponse.value[0]["reportid"];
            }
            return lValue;
        },
        ExecuteReport: function (reportFileName, reportPrefilter) {
            let reportGuid = this.GetReportIdByReportFileName(reportFileName);
            let pth = this.GetClientUrl() + "/CRMReports/rsviewer/ReportViewer.aspx";
            let orgUniqueName = Xrm.Utility.getGlobalContext().getOrgUniqueName();
            let query = "id=%7B" + reportGuid +
                "%7D&uniquename=" + orgUniqueName +
                "&iscustomreport=true&reportnameonsrs=&reportName=" + reportFileName +
                "&isScheduledReport=false&p:" + reportPrefilter;
            let retrieveEntityReq = new XMLHttpRequest();
            retrieveEntityReq.open("POST", pth, false);
            retrieveEntityReq.setRequestHeader("Accept", "*/*");
            retrieveEntityReq.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            retrieveEntityReq.send(query);
            let x = retrieveEntityReq.responseText.lastIndexOf("ReportSession=");
            let y = retrieveEntityReq.responseText.lastIndexOf("ControlID=");
            let ret = [];
            ret[0] = retrieveEntityReq.responseText.slice(x + 14, x + 14 + 24);
            ret[1] = retrieveEntityReq.responseText.slice(y + 10, y + 10 + 32);
            return ret;
        },
        /**
         * 
         * @param {any} arrResponseSession
         * @param {any} lcId (Language code)
         * @returns
         */
        Get_SSRS_Report_PDFBase64: function (arrResponseSession, lcId) {
            let that = this;
            return new Promise(function (resolve, reject) {
                let pth = that.GetClientUrl() + "/Reserved.ReportViewerWebControl.axd?ReportSession=" + arrResponseSession[0] + "&Culture=" + lcId + "&CultureOverrides=True&UICulture=" + lcId + "&UICultureOverrides=True&ReportStack=1&ControlID=" + arrResponseSession[1] + "&OpType=Export&FileName=Public&ContentDisposition=OnlyHtmlInline&Format=PDF";
                let retrieveEntityReq = new XMLHttpRequest();
                retrieveEntityReq.open("GET", pth, true);
                retrieveEntityReq.setRequestHeader("Accept", "*/*");
                retrieveEntityReq.responseType = "arraybuffer";
                retrieveEntityReq.onreadystatechange = function () {
                    if (retrieveEntityReq.readyState == 4 && retrieveEntityReq.status == 200) {
                        let binary = "";
                        let bytes = new Uint8Array(this.response);
                        for (let i = 0; i < bytes.byteLength; i++) {
                            binary += String.fromCharCode(bytes[i]);
                        }
                        let base64PDFString = btoa(binary);
                        resolve(base64PDFString);
                    }
                };
                retrieveEntityReq.send();
            });
        },
        GetClientUrl: function () {
            let lGlobalContext = "";
            try {
                lGlobalContext = Xrm.Utility.getGlobalContext();
            }
            catch (e) {
                lGlobalContext = parent.Xrm.Utility.getGlobalContext();
            }

            if (lGlobalContext !== null) {
                return lGlobalContext.getClientUrl();
            }
            return null;
        },
        RetrieveMultipleRecord: function (lEntityName, lFilter, lCommaSeparatedAttributeNames, isAdmin) {
            let lResponse = null;
            let lXMLHttpRequest = new XMLHttpRequest();
            lXMLHttpRequest.open("GET", this.GetClientUrl() + "/api/data/v9.2/" + lEntityName + "?$select=" + lCommaSeparatedAttributeNames + "&$filter=" + lFilter, false);
            lXMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
            lXMLHttpRequest.setRequestHeader("OData-Version", "4.0");
            lXMLHttpRequest.setRequestHeader("Accept", "application/json");
            lXMLHttpRequest.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            lXMLHttpRequest.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
            // If IsAdmin is true, it is executed as an administrator
            if (isAdmin) {
                lXMLHttpRequest.setRequestHeader("MSCRMCallerID", this.GetConfigurationValue(this.Constants.SystemAdminId));
            }
            lXMLHttpRequest.onreadystatechange = function () {
                if (this.readyState === 4) {
                    lXMLHttpRequest.onreadystatechange = null;
                    if (this.status === 200) {
                        lResponse = JSON.parse(this.response);
                    } else {
                        Xrm.Navigation.openAlertDialog("An exception has occurred, please contact the system administrator.");
                        console.log("Error:");
                        console.log(this.statusText);
                    }
                }
            };
            lXMLHttpRequest.send();
            return lResponse;
        },
        RetrieveSingleRecord: function (lEntityName, lEntityId, lCommaSeparatedAttributeNames, admin) {
            let lResponse = null;
            let lXMLHttpRequest = new XMLHttpRequest();
            lXMLHttpRequest.open("GET", this.GetClientUrl() + "/api/data/v9.2/" + lEntityName + "(" + lEntityId + ")" + "?$select=" + lCommaSeparatedAttributeNames, false);
            lXMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
            lXMLHttpRequest.setRequestHeader("OData-Version", "4.0");
            lXMLHttpRequest.setRequestHeader("Accept", "application/json");
            lXMLHttpRequest.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            lXMLHttpRequest.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
            if (admin) {
                lXMLHttpRequest.setRequestHeader("MSCRMCallerID", this.GetConfigurationValue(this.Constants.SystemAdminId));
            }
            lXMLHttpRequest.onreadystatechange = function () {
                if (this.readyState === 4) {
                    lXMLHttpRequest.onreadystatechange = null;
                    if (this.status === 200) {
                        lResponse = JSON.parse(this.response);
                    } else {
                        Xrm.Navigation.openAlertDialog("An exception has occurred, please contact the system administrator.");
                        console.log("Error:");
                        console.log(this.statusText);
                    }
                }
            };
            lXMLHttpRequest.send();
            return lResponse;
        },
        /**
         * Get Configuration Value
         * P.S: This is my own new configuration entity, there are two main fields: (1) name , (2) value.
         * if you need to use, please create and modify the following field information
         * @param {any} configName
         * @returns
         */
        GetConfigurationValue: function (configName) {
            let lValue = "";
            let lResponse = this.RetrieveMultipleRecord("Your Config Entity Logical Collection Name", "gdh_name eq '" + configName + "'", "gdh_value");
            if (lResponse !== null && lResponse !== undefined && lResponse.value.length > 0) {
                lValue = lResponse.value[0][this.Constants.ConfigurationsField.Value];
            }
            return lValue;
        },
        /**
         * App消息提醒
         * @param {any} str_title
         * @param {any} str_body
         * @param {any} str_ownerId
         * @param {any} int_IconType
         * @param {any} int_ToastType
         * @param {any} json_Data
         */
        CreateAppNotification: function (str_title, str_body, str_ownerId, int_IconType, int_ToastType, json_Data) {
        let appNotificationRequest = {
            title: str_title,
            body: str_body,
            'ownerid@odata.bind': '/systemusers(' + str_ownerId + ')',
            icontype: int_IconType,
            toasttype: int_ToastType,
            data: json_Data,
        };
        Xrm.WebApi.createRecord('appnotification', appNotificationRequest)
            .then((result) => {
                console.log('notification registered with ID: ' + result.id);
            })
            .catch((ex) => console.error(`error message: ${ex.message}`));
    }
    }
})();