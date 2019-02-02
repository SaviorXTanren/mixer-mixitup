var commands = [],
    selectedCommandId = null;

function loadAction() {
    websocket.onmessage = function (evt) {
        // Received message from Stream Deck
        var jsonObj = JSON.parse(evt.data);

        if (jsonObj.event === 'sendToPropertyInspector') {
            var command_selection = document.getElementById('command_selection');
            command_selection.style.display = "none";

            var payload = jsonObj.payload;
            if (checkForErrors(payload)) {
                return;
            }

            command_selection.style.display = "";

            // Save the payload
            commands = payload.commands;
            selectedCommandId = payload.selectedCommandId;
            sortCommands();

            var selectCategoryDiv = document.getElementById('selected_category');
            var selectCategory = selectCategoryDiv.querySelector('.sdpi-item-value');
            removeOptions(selectCategory);

            var curGroupName;
            for (var index = 0; index < commands.length; index++) {
                if (curGroupName !== commands[index].Category) {
                    curGroupName = commands[index].Category;

                    var curGroup = document.createElement('option');
                    curGroup.text = curGroupName;
                    curGroup.value = curGroupName;
                    selectCategory.appendChild(curGroup);
                }
            }

            var argsDiv = document.getElementById('arguments');
            var args = argsDiv.querySelector('.sdpi-item-value');
            args.value = payload.arguments;

            selectCategory.disabled = false;
            args.disabled = false;

            refreshSelectedCommand();
        }
    };
}

function sortCommands() {
    for (var index = 0; index < commands.length; index++) {
        if (commands[index].GroupName === "") {
            commands[index].GroupName = "Ungrouped";
        }
    }

    commands.sort(function (a, b) {
        if (a.Category === b.Category) {
            return a.GroupName > b.GroupName ? 1 : -1;
        }

        return a.Category > b.Category ? 1 : -1;
    });
}

function refreshSelectedCommand() {
    var selectCategoryDiv = document.getElementById('selected_category');
    var selectCategory = selectCategoryDiv.querySelector('.sdpi-item-value');

    var selectCommandDiv = document.getElementById('selected_command');
    var selectCommand = selectCommandDiv.querySelector('.sdpi-item-value');
    removeOptions(selectCommand);

    if (selectedCommandId) {
        for (var index = 0; index < commands.length; index++) {
            if (commands[index].ID === selectedCommandId) {
                selectCategory.value = commands[index].Category;
                break;
            }
        }
    }

    var curGroupName = null;
    var curGroup = null;
    for (index = 0; index < commands.length; index++) {
        if (commands[index].Category !== selectCategory.value) {
            continue;
        }

        if (commands[index].GroupName && commands[index].GroupName !== "" && curGroupName !== commands[index].GroupName) {
            curGroupName = commands[index].GroupName;

            curGroup = document.createElement('optgroup');
            curGroup.label = curGroupName;
            selectCommand.appendChild(curGroup);
        }

        var opt = document.createElement('option');
        opt.value = commands[index].ID;
        opt.text = commands[index].Name;

        if (curGroup) {
            curGroup.appendChild(opt);
        } else {
            selectCommand.appendChild(opt);
        }
    }

    selectCommand.disabled = false;
    if (selectedCommandId) {
        selectCommand.value = selectedCommandId;
    } else {
        selectCommand.value = null;
    }
}

function updateCategory() {
    // Clear to reset
    selectedCommandId = null;
    refreshSelectedCommand();
}

function updateSettings() {
    var selectDiv = document.getElementById('selected_command');
    var selectCommand = selectDiv.querySelector('.sdpi-item-value');

    var argsDiv = document.getElementById('arguments');
    var args = argsDiv.querySelector('.sdpi-item-value');

    var payload = {};
    payload.property_inspector = 'updateSettings';
    payload.selectedCommandId = selectCommand.value;
    payload.arguments = args.value;
    sendPayloadToPlugin(payload);
}
