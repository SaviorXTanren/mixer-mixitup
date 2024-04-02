/**
 * Copyright 2021-2022 AVerMedia Technologies Inc. and respective authors and developers.
 * This source code is licensed under the MIT-style license found in the LICENSE file.
 *
 * https://github.com/AVerMedia-Technologies-Inc/CreatorCentralSDK/blob/main/RegistrationFlow.md
 *
 * Include this script AFTER ax_property.js
 */

const setWidgetSettings = AVT_CREATOR_CENTRAL_API_V2.setWidgetSettings;
const openUrl = AVT_CREATOR_CENTRAL_API_V2.openUrl;
const changeWidgetTitle = AVT_CREATOR_CENTRAL_API_V2.changeWidgetTitle;

let widgetUuid = "";
let widgetName = "";

AVT_CREATOR_CENTRAL.on('webSocketConnected', data => {
    let port = data["port"];
    let uuid = data["uuid"];
    let widgetInfo = data["widget"];

    widgetUuid = widgetInfo["context"];
    widgetName = widgetInfo["widget"];
});

AVT_CREATOR_CENTRAL.on('didReceiveWidgetSettings', data => {
    let payload = data["payload"];

    const args = (payload["commandArgs"] && payload["commandArgs"].length) ? payload["commandArgs"][0] : '';

    if (payload["commandId"] != null && payload["commandType"] != null ) {
        $("#js_commandType").val(payload["commandType"]);

        loadCommandsOfType(payload["commandType"]);
        $("#js_commandId").val(payload["commandId"]);

        $("#js_commandArgs").val(args);
    }
});

function getHelp() {
    openUrl("https://wiki.mixitupapp.com/services/developer-api");
}

function saveWidgetSettings() {
    // refresh value from GUI
    const commandType = $("#js_commandType").val();
    const commandId = $("#js_commandId").val();
    const commandArgs = $("#js_commandArgs").val();

    let widgetJson = {
        "commandType": commandType,
        "commandId": commandId,
        "commandArgs": [ commandArgs ],
    };
    setWidgetSettings(widgetName, widgetUuid, widgetJson);
}

function commandSorter(a, b) {
    const catA = a.Category.toUpperCase(); // Ignore case when sorting
    const catB = b.Category.toUpperCase(); // Ignore case when sorting
    if (catA < catB) {
      return -1;
    }
    if (catA > catB) {
      return 1;
    }

    const nameA = a.Name.toUpperCase(); // Ignore case when sorting
    const nameB = b.Name.toUpperCase(); // Ignore case when sorting
    if (nameA < nameB) {
      return -1;
    }
    if (nameA > nameB) {
      return 1;
    }

    return 0;
}

var allCommands = [];
function loadCommands(commands) {
    commands.sort(commandSorter);

    allCommands = commands;

    $('#js_commandType').empty();

    var uniqueTypes = [];
    for (const command of commands) {
        if ($.inArray(command.Category, uniqueTypes) === -1) {
            uniqueTypes.push(command.Category);
            $('#js_commandType').append($('<option>', {
                value: command.Category,
                text: command.Category
            }));
        }
    }

    loadCommandsOfType(uniqueTypes[0]);
}

function loadCommandsOfType(currentType) {
    $('#js_commandId').empty();

    for (const command of allCommands) {
        if (currentType === command.Category) {
            $('#js_commandId').append($('<option>', {
                value: command.ID,
                text: command.Name
            }));
        }
    }
}

function updateArguments() {
    saveWidgetSettings();
}

$(document).ready(function(){
    $("#js_commandId").change(saveWidgetSettings);

    $('#js_commandType').on('change', function() {
        var selectedValue = $(this).val();
        loadCommandsOfType(selectedValue);
    });

    $.get("http://localhost:8911/api/commands", function(data) {
        $("#loading").removeAttr("style").hide();
        $("#error").removeAttr("style").hide();

        loadCommands(data);

        $("#options").show();
    })
    .fail(function() {
        $("#loading").removeAttr("style").hide();
        $("#options").removeAttr("style").hide();

        $("#error").show();
    });
});

/**
 * Creator Central entry point
 */
function connectCreatorCentral(port, uuid, inEvent, inInfo, inWidgetInfo) {
    AVT_CREATOR_CENTRAL.connect(port, uuid, inEvent, inInfo, inWidgetInfo);
}
