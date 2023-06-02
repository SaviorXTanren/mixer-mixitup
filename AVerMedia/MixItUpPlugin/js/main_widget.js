/**
 * Copyright 2021-2022 AVerMedia Technologies Inc. and respective authors and developers.
 * This source code is licensed under the MIT-style license found in the LICENSE file.
 */

const setWidgetSettings = AVT_CREATOR_CENTRAL_API_V2.setWidgetSettings;
const changeWidgetImage = AVT_CREATOR_CENTRAL_API_V2.changeWidgetImage;

let configData = {}; // configs that need to stored in Creator Central
let drawerTimer = -1;

function refreshWidgetUi(uuid, config) {
    let widget = config["widget"];
    changeWidgetImage(widget, uuid, image);
}

/**
 * Event received after sending the getWidgetSettings event to retrieve
 * the persistent data stored for the widget.
 */
AVT_CREATOR_CENTRAL.on('didReceiveWidgetSettings', data => {
    let widget = data["widget"];
    let uuid = data["context"];
    let payload = data["payload"];

    configData[uuid] = payload; // cache the settings for future use
    configData[uuid].widget = widget; // pass widget name in json
    refreshWidgetUi(uuid, configData[uuid]); // refresh widget if necessary
});

/**
 * When an instance of a widget is displayed on Creator Central, for example,
 * when the profile loaded, the package will receive a willAppear event.
 */
AVT_CREATOR_CENTRAL.on('widgetWillAppear', data => {
    let uuid = data["context"];

    configData[uuid] = {};
});

/**
 * When switching profile, an instance of a widget will be invisible,
 * the package will receive a willDisappear event.
 */
AVT_CREATOR_CENTRAL.on('widgetWillDisappear', data => {
    let uuid = data["context"];

    delete configData[uuid];
    // stop the drawer timer
    if (Object.entries(configData) <= 0) { // no widgets need to be updated
        clearInterval(drawerTimer); // stop the timer
        drawerTimer = -1; // reset drawer timer
    }
});

/**
 * When the user presses a display view and then releases within it, the package
 * will receive the widgetTriggered event.
 */
AVT_CREATOR_CENTRAL.on('actionTriggered', data => {
    let widget = data["widget"];
    let uuid = data["context"];

    // make sure the widget is clickable
    if (configData[uuid] != null && configData[uuid].commandId != null) {
        const args = configData[uuid].commandArgs == null ? [] : configData[uuid].commandArgs;

        $.ajax({
            type: 'POST',
            url: `http://localhost:8911/api/commands/${configData[uuid].commandId}`,
            data: JSON.stringify(args),
            contentType: 'application/json'});

        setWidgetSettings(widget, uuid, widgetJson); // it will trigger didReceiveWidgetSettings
    }
});

/**
 * Creator Central entry point
 */
function connectCreatorCentral(port, uuid, inEvent, inInfo) {
    AVT_CREATOR_CENTRAL.connect(port, uuid, inEvent, inInfo, null);
}
