/**
 * [Example Code]
 * Account Entity JavaScript
 */
'use strict';
if (Gdh === undefined) { var Gdh = {}; }
if (Gdh.D365 === undefined) { Gdh.D365 = {}; }
if (Gdh.D365.Account === undefined) { Gdh.D365.Account = {}; }
(function () {
    this.formOnLoad = function (executionContext) {
        //var formContext = executionContext.getFormContext();
    };

    this.formOnSave = function () {
        Xrm.Navigation.openAlertDialog({ text: "Record saved." });
    };

    this.ExampleCallAppNotification = function (executionContext) {
        var formContext = executionContext;
        var currentUserID = Xrm.Utility.getGlobalContext().userSettings.userId.replace("{", "").replace("}", "");
        var accountName = formContext.getAttribute("name").getValue();
        var sBody = `Customer [**${accountName}**] has been assigned to you, please contact the customer in time.`;
        var currentRecordId = formContext.data.entity.getId().replace("{", "").replace("}", "");
        var url = `?pagetype=entityrecord&etn=account&id=${currentRecordId}`;
        var tData = {
            "@odata.type": "Microsoft.Dynamics.CRM.expando",
            "actions@odata.type": "#Collection(Microsoft.Dynamics.CRM.expando)",
            "actions": [
                {
                    "title": "Open Account record",
                    "data": {
                        "@odata.type": "#Microsoft.Dynamics.CRM.expando",
                        "type": "url",
                        "url": url,
                        "navigationTarget": "newWindow"
                    }
                }
            ]
        };
        var overrideContent = {
            "@odata.type": "#Microsoft.Dynamics.CRM.expando",
            "title": "**(Override)Client assignment reminders**",
            "body": `Customer [${accountName}](${url}) has been assigned to you, _please contact the customer in time_.`
        };
        var SendAppNotificationRequest = new this.SendAppNotificationRequest(
            "Client assignment reminders",
            `/systemusers(${currentUserID})`,
            sBody,
            200000000,
            100000001,
            200000000,
            null,
            overrideContent,
            tData
        );
        Xrm.WebApi.online.execute(SendAppNotificationRequest).then(function (response) {
            if (response.ok) {
                console.log("Status: %s %s", response.status, response.statusText);

                return response.json();
            }
        })
            .then(function (responseBody) {
                console.log("Response Body: %s", responseBody.NotificationId);
            })
            .catch(function (error) {
                console.log(error.message);
            });
    };

    this.SendAppNotificationRequest = function (
        title,
        recipient,
        body,
        priority,
        iconType,
        toastType,
        expiry,
        overrideContent,
        actions) {
        this.Title = title;
        this.Recipient = recipient;
        this.Body = body;
        this.Priority = priority;
        this.IconType = iconType;
        this.ToastType = toastType;
        this.Expiry = expiry;
        this.OverrideContent = overrideContent;
        this.Actions = actions;
    };

    this.SendAppNotificationRequest.prototype.getMetadata = function () {
        return {
            boundParameter: null,
            parameterTypes: {
                "Title": {
                    "typeName": "Edm.String",
                    "structuralProperty": 1
                },
                "Recipient": {
                    "typeName": "mscrm.systemuser",
                    "structuralProperty": 5
                },
                "Body": {
                    "typeName": "Edm.String",
                    "structuralProperty": 1
                },
                "Priority": {
                    "typeName": "Edm.Int",
                    "structuralProperty": 1
                },
                "IconType": {
                    "typeName": "Edm.Int",
                    "structuralProperty": 1
                },
                "ToastType": {
                    "typeName": "Edm.Int",
                    "structuralProperty": 1
                },
                "Expiry": {
                    "typeName": "Edm.Int",
                    "structuralProperty": 1
                },
                "OverrideContent": {
                    "typeName": "mscrm.expando",
                    "structuralProperty": 5
                },
                "Actions": {
                    "typeName": "mscrm.expando",
                    "structuralProperty": 5
                },
            },
            operationType: 0,
            operationName: "SendAppNotification",
        }
    };
    this.ExampleRandomlyCreateContactAction = function () {
        var inputActionParams = {
            // 本次示例不需要传入参数
        };
        Xrm.Utility.showProgressIndicator();
        setTimeout(function () {
            let actionResult = this.ExecuteAction("gdh_ExampleRandomlyCreateContactAction", inputActionParams);
            Xrm.Utility.closeProgressIndicator();
            Xrm.Page.data.refresh();
            for (let index = 0; index < window.parent.length; index++) {
                if (window.parent[index] &&
                    window.parent[index].Xrm &&
                    window.parent[index].Xrm.Page &&
                    window.parent[index].Xrm.Page.data &&
                    window.parent[index].Xrm.Page.data.refresh) {
                    window.parent[index].Xrm.Page.data.refresh();
                    break;
                }
            }
        }, 1000);
    };
    this.ExecuteAction = function (actionUrl, actionPara, admin) {
        var lResponse = null;
        var lXMLHttpRequest = new XMLHttpRequest();
        lXMLHttpRequest.open("POST", encodeURI(this.GetClientUrl() + "/api/data/v9.2/" + actionUrl), false);
        lXMLHttpRequest.setRequestHeader("Accept", "application/json");
        lXMLHttpRequest.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        lXMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
        lXMLHttpRequest.setRequestHeader("OData-Version", "4.0");
        if (admin) {
            lXMLHttpRequest.setRequestHeader("MSCRMCallerID", "<Your SystemAdmin Id>");
        }
        if (this.IsNullOrUndefined(actionPara)) {
            lXMLHttpRequest.send();
        }
        else {
            lXMLHttpRequest.send(JSON.stringify(actionPara));
        }
        if (lXMLHttpRequest.status === 200) {
            lResponse = JSON.parse(lXMLHttpRequest.responseText);
        }
        else if (lXMLHttpRequest.status === 400) {
            lResponse = JSON.parse(lXMLHttpRequest.responseText);
        }
        else {
            if (lXMLHttpRequest.statusText !== null && lXMLHttpRequest.statusText !== undefined && lXMLHttpRequest.statusText !== "") {
                Xrm.Navigation.openAlertDialog(lXMLHttpRequest.statusText);
            }
        }
        return lResponse;
    };
    this.GetClientUrl = function () {
        var lGlobalContext = "";
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
    };
}).call(Gdh.D365.Account);
