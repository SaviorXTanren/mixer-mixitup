/**
 * Copyright 2021-2022 AVerMedia Technologies Inc. and respective authors and developers.
 * This source code is licensed under the MIT-style license found in the LICENSE file.
 *
 * https://github.com/AVerMedia-Technologies-Inc/CreatorCentralSDK/blob/main/RegistrationFlow.md
 */

window.AVT_CREATOR_CENTRAL;

WebSocket.prototype.sendJSON = function(json) {
    this.send(JSON.stringify(json));
};

class EventEmitter {
    constructor() {
        this.events = {};
    }
    
    on(event, listener) {
        if (typeof this.events[event] !== 'object') {
            this.events[event] = [];
        }
        this.events[event].push(listener);
        return () => this.removeListener(event, listener);
    }
    
    removeListener(event, listener) {
        if (typeof this.events[event] === 'object') {
            const idx = this.events[event].indexOf(listener);
            if (idx > -1) {
                this.events[event].splice(idx, 1);
            }
        }
    }
    
    emit(event, ...args) {
        if (typeof this.events[event] === 'object') {
            this.events[event].forEach(listener => listener.apply(this, args));
        }
    }
    
    once(event, listener) {
        const remove = this.on(event, (...args) => {
            remove();
            listener.apply(this, args);
        });
    }
};

const AVT_CREATOR_CENTRAL_API_V2 = {
    send: function (apiType, payload, widget, uuid) {
        let context = uuid != null ? uuid: AVT_CREATOR_CENTRAL.uuid;
        let pl = {
            event : apiType,
            context : context
        };
        if (payload) {
            pl.payload = payload;
        }
        if (widget) {
            pl.widget = widget;
        }

        AVT_CREATOR_CENTRAL.connection && AVT_CREATOR_CENTRAL.connection.sendJSON(pl);
    },

    setWidgetSettings: function(widget, uuid, payload) {
        AVT_CREATOR_CENTRAL_API_V2.send('setWidgetSettings', payload, widget, uuid);
    },
    changeWidgetImage: function (widget, uuid, image) {
        let payload = {"image": image};
        AVT_CREATOR_CENTRAL_API_V2.send('changeImage', payload, widget, uuid);
    },
    changeWidgetTitle: function (widget, uuid, title) {
        let payload = {"title": title, "state": 0};
        AVT_CREATOR_CENTRAL_API_V2.send('changeTitle', payload, widget, uuid);
    },
    openUrl: function(url) {
        AVT_CREATOR_CENTRAL_API_V2.send('openUrl', { url: url });
    },
};

// main
AVT_CREATOR_CENTRAL = (function() {
    function parseJson (jsonString) {
        if (typeof jsonString === 'object') return jsonString;
        try {
            const o = JSON.parse(jsonString);
            if (o && typeof o === 'object') {
                return o;
            }
        } catch (e) {}
        return false;
    }

    function init() {
        let inPort, inUuid, inEvent, inPackageInfo, inWidgetInfo;
        let websocket = null;
        let events = new EventEmitter();

        function connect(port, uuid, event, info, widgetInfo) {
            inPort = port;
            inUuid = uuid;
            inEvent = event;
            inPackageInfo = info;
            inWidgetInfo = widgetInfo;
            
            websocket = new WebSocket(`ws://localhost:${inPort}`);

            websocket.onopen = function() {
                let json = {
                    event : inEvent,
                    uuid: inUuid
                };
                websocket.sendJSON(json);
                
                AVT_CREATOR_CENTRAL.uuid = inUuid;
                AVT_CREATOR_CENTRAL.connection = websocket;
                
                events.emit('webSocketConnected', {
                    port: inPort,
                    uuid: inUuid,
                    pkgInfo: inPackageInfo,
                    widget: inWidgetInfo,
                    connection: websocket
                });
            };

            websocket.onerror = function(evt) {
                console.warn('WEBSOCKET ERROR', evt, evt.data);
            };

            websocket.onclose = function(evt) {
                console.warn('error', evt); // Websocket is closed
            };

            websocket.onmessage = function(evt) {
                if (evt.data) {
                    let jsonObj = parseJson(evt.data);
                    events.emit(jsonObj.event, jsonObj);
                }
            };
        }

        return {
            connect: connect,
            on: (event, callback) => events.on(event, callback),
            emit: (event, callback) => events.emit(event, callback)
        };
    }

    return init();
})();
