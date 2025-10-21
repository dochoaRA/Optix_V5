function masterPageStartUp(callback) {
    require([ "dojo/parser", "dojo/query", "dijit/registry", "dojo/on", "dojo/_base/fx", "dojo/dom-construct", "dojo/domReady!"],
    function (pars, query, registry, on, fx, cst) {
        pars.parse().then(function () {
            query('body').forEach(function (node) {
                node.style.backgroundImage = 'none';
            });
            query('.body').forEach(function (node) {
                node.style.backgroundImage = 'none';
            });
            query('.page').forEach(function (form) {
                fx.fadeIn({
                    node: form, duration: 50
                }).play();
            });
            var t = registry.byId('_searchfield');
            if (t == null || typeof (t) == 'undefined')
            t = query('#_searchfield' + target)[0];
            on(t, "change", function (value) {
                markContent(value);
                if (t._timer) clearTimeout(t._timer);
                t._timer = setTimeout(function () {
                    if (! t._timer) return;
                    search();
                },
                100);
            });
        });
        if (callback != null && typeof (callback) != 'undefined')
        callback();
    });
}

function initBaseFunctions(callback) {
    navigation.index = {
    };
    for (var i = 0, len = navigation.length; i < len; i++) {
        navigation.index[navigation[i].id] = navigation[i];
    }
    partmap.index = {
    };
    for (var i = 0, len = partmap.length; i < len; i++) {
        partmap.index[partmap[i].id] = partmap[i];
    }
    contentmap.index = {
    };
    for (var i = 0, len = contentmap.length; i < len; i++) {
        contentmap.index[contentmap[i].id] = contentmap[i];
    }
    searchindex.index = {
    };
    for (var i = 0, len = searchindex.length; i < len; i++) {
        searchindex.index[searchindex[i].id] = searchindex[i];
    }
    searchtext.index = {
    };
    for (var i = 0, len = searchtext.length; i < len; i++) {
        searchtext.index[searchtext[i].id] = searchtext[i];
    }
    customprop.index = {
    };
    for (var i = 0, len = customprop.length; i < len; i++) {
        customprop.index[customprop[i].id] = customprop[i];
    }
    if (! Array.prototype.filter) {
        Array.prototype.filter = function (fun) {
            "use strict";
            
            if (this == null)
            throw new TypeError();
            
            var t = Object(this);
            var len = t.length >>> 0;
            if (typeof fun != "function")
            throw new TypeError();
            
            var res =[];
            var thisp = arguments[1];
            for (var i = 0; i < len; i++) {
                if (i in t) {
                    var val = t[i];
                    if (fun.call(thisp, val, i, t))
                    res.push(val);
                }
            }
            
            return res;
        };
    }
    if (! Array.prototype.indexOf) {
        Array.prototype.indexOf = function (searchElement) {
            "use strict";
            if (this == null) {
                throw new TypeError();
            }
            var t = Object(this);
            var len = t.length >>> 0;
            if (len === 0) {
                return -1;
            }
            var n = 0;
            if (arguments.length > 0) {
                n = Number(arguments[1]);
                if (n != n) {
                    n = 0;
                } else if (n != 0 && n != Infinity && n != - Infinity) {
                    n = (n > 0 || -1) * Math.floor(Math.abs(n));
                }
            }
            if (n >= len) {
                return -1;
            }
            var k = n >= 0 ? n: Math.max(len - Math.abs(n), 0);
            for (; k < len; k++) {
                if (k in t && t[k] === searchElement) {
                    return k;
                }
            }
            return -1;
        }
    }
    var JSON = JSON || {
    };
    if (! JSON.stringify) {
        JSON.stringify = function (obj) {
            var t = typeof (obj);
            if (t != "object" || obj === null) {
                if (t == "string")
                obj = '"' + obj + '"';
                return String(obj);
            } else {
                var n, v, json =[], arr = (obj && obj.constructor == Array);
                
                for (n in obj) {
                    v = obj[n];
                    t = typeof (v);
                    if (t == "string") v = '"' + v + '"'; else if (t == "object" && v !== null) v = JSON.stringify(v);
                    json.push((arr ? "": '"' + n + '":') + String(v));
                }
                return (arr ? "[": "{") + String(json) + (arr ? "]": "}");
            }
        };
    }
    if (! JSON.parse) {
        JSON.parse = JSON.parse || function (str) {
            if (str === "")
            str = '""';
            eval("var p=" + str + ";");
            return p;
        };
    }
    if (callback != null && typeof (callback) != 'undefined')
    callback();
}

function loadRotation(callback) {
    var rotates = document.getElementsByClassName('rotate_outer');
    for (var i = 0; i < rotates.length; i++) {
        var inner = rotates[i].getElementsByClassName('rotate')[0];
        rotates[i].style.width = inner.offsetHeight + 'px';
        
        if (inner.className.indexOf("d3") !== -1) {
            if (rotates[i].style.transform !== undefined)
            rotates[i].style.transform = 'translate(-' +(inner.offsetWidth - inner.offsetHeight) + 'px)'; else
            rotates[i].style.msTransform = 'translate(-' +(inner.offsetWidth - inner.offsetHeight) + 'px)';
        }
    }
    rotates = document.getElementsByClassName('rotate');
    for (var i = 0; i < rotates.length; i++) {
        if (rotates[i].className.indexOf("d2") === -1) {
            rotates[i].style.height = rotates[i].offsetWidth + 'px';
        }
    }
    if (callback != null && typeof (callback) != 'undefined')
    callback();
}


function loadPage() {
    require([ "dojo/store/Memory", "dojo/io-query"],
    function (Memory, ioQuery) {
        var uri = window.location.search;
        var query = ioQuery.queryToObject(uri.substring(uri.indexOf("?") + 1, uri.length));
        var store = new Memory({
            data: partmap
        });
        var part = loadPart(query, store);
        
        loadContentInto('_header', 'header_' + part, function () {
            loadContentInto('_menu', 'menu_' + part, function () {
                loadTreeInto('_nav', 'navigation', part,
                function (item) {
                    loadContentInto('_content', item);
                },
                function () {
                    loadTreeInto('_index', 'searchindex', part,
                    function (item) {
                        loadIndex(item);
                    },
                    function () {
                        loadStartPage(part, query, function () {
                            masterPageStartUp(function () {
                                loadPartSelect(store);
                            });
                        });
                    });
                });
            });
        });
    });
}

function loadStartPage(part, query, callback) {
    if (query[ 'cst'] != null && query[ 'cst'].length > 0)
    search(query[ 'cst'], callback); else if (query[ 'id'] != null && query[ 'id'].length > 0)
    loadContentInto('_content', query[ 'id'], callback); else
    loadContentInto('_content', part, callback);
}

function loadPart(query, parts) {
    var part = '';
    if (query[ 'part'] != null && query[ 'part'].length > 0)
    part = query[ 'part'];
    
    var activePart = parts.query(function (item) {
        return item.name == part || item.id == part;
    });
    if (activePart.total == 0)
    part = parts.data[0][ 'id']; else
    part = activePart[0][ 'id'];
    return part;
}

function loadPartSelect(store, callback) {
    require([ "dojo/store/Memory", "dijit/PopupMenuBarItem", "dijit/MenuBarItem", "dijit/MenuItem", "dijit/DropDownMenu", "dijit/registry", "dojo/on", "dojo/dom-style", "dojo/query", "dojo/dom-construct", "dojo/domReady!"],
    function (Memory, PopupMenuBarItem, MenuBarItem, MenuItem, DropDownMenu, registry, on, style, query, cst) {
        if (store.data.length > 3) {
            var t = registry.byId('partDropDown');
            if (t == null || typeof (t) == 'undefined')
            t = query('#partDropDown')[0];
            var node = t.domNode;
            if (node == null || typeof (node) == 'undefined')
            node = t;
            dojo.forEach(store.data, function (data) {
                var item = new MenuItem({
                    label: data[ 'name']
                });
                on(item, 'click', function () {
                    window.location.replace("?part=" + data[ 'name']);
                });
                cst.place(item.domNode, node);
            });
        } else {
            var t = registry.byId('partSelection');
            if (t == null || typeof (t) == 'undefined')
            t = query('#partSelection')[0];
            
            style.set('partSelection', "display", "none");
            dojo.forEach(store.data, function (data) {
                var item = new MenuBarItem({
                    label: data[ 'name']
                });
                on(item, 'click', function () {
                    window.location.replace("?part=" + data[ 'name']);
                });
                style.set(item.domNode, 'float', 'right');
                var parent = t.parentNode;
                if (parent == null || typeof (parent) == 'undefined')
                parent = t.domNode.parentNode;
                
                parent.appendChild(item.domNode);
            });
        }
        if (callback != null && typeof (callback) != 'undefined')
        callback();
    });
}

function getContentId(id, callback) {
    require([ "dojo/parser", "dojo/store/Memory", "dojo/domReady!"],
    function (parser, Memory) {
        var partStore = new Memory({
            data: partmap
        });
        var contentStore = new Memory({
            data: contentmap
        });
        var result = contentStore.query(function (item) {
            return item.id == id || item.references.indexOf(id) != -1;
        });
        if (result.length == 0) {
            result = partStore.query(function (item) {
                return item.id == id || item.name == id || item.references.indexOf(id) != -1;
            });
        }
        if (callback != null && typeof (callback) != 'undefined')
        callback(result.length != 0 ? result[0][ 'id']: id);
    });
}

function getContent(id, json, callback) {
    require([ "dojo/query", "dijit/registry", "dojo/on", "dojo/dom-construct", "dojo/_base/sniff", "dojo/domReady!"],
    function (query, registry, on, cst) {
        var path = json ? './config/' + id + '.html': './sites/' + id + '.html';
        var url = require.toUrl(path);
        
        var loader = registry.byId('bgLoader');
        if (loader == null || typeof (loader) == 'undefined')
        loader = query('#bgLoader')[0];
        
        cst.empty(loader);
        var onLoad = function (content) {
            if (window.postMessage) {
                var contentData = content.data;
                var data;
                if (contentData.id)
                data = contentData; else {
                    data = eval('(' + contentData + ')');
                    data.value = unescape(data.value);
                }
                var target = data.id;
                if (target == id) {
                    clearTimeout(timer);
                    if (window.removeEventListener)
                    window.removeEventListener('message', onLoad, false); else if (window.detachEvent)
                    window.detachEvent("onmessage", onLoad); else {
                        callback('Browser does not support HTML Event API');
                        return;
                    }
                    cst.empty(loader);
                    if (json) {
                        var cb = eval('(' + data.value + ')');
                        var ret = cb.filter(function (item) {
                            return item != null && item.id != null;
                        });
                        callback(ret);
                    } else
                    callback(data);
                }
            } else {
                clearTimeout(timer);
                if (window.removeEventListener)
                window.removeEventListener('message', onLoad, false); else if (window.detachEvent)
                window.detachEvent("onmessage", onLoad); else
                callback('Browser does not support HTML Event API');
                return;
                callback('Browser does not support function \'postMessage\'');
            }
        };
        if (window.addEventListener)
        window.addEventListener('message', onLoad, false); else if (window.attachEvent)
        window.attachEvent("onmessage", onLoad); else {
            callback('Browser does not support HTML Event API');
            return;
        }
        
        var timer = setTimeout(function () {
            clearTimeout(timer);
            if (window.removeEventListener)
            window.removeEventListener('message', onLoad, false); else if (window.detachEvent)
            window.detachEvent("onmessage", onLoad); else {
                callback('Browser does not support HTML Event API');
                return;
            }
            callback('Resource \'' + url + '\' not found');
        },
        10000);
        
        var iframe = cst.create('iframe', {
            className: 'dijitOffScreen',
            src: url
        },
        loader);
    });
}

function loadHTML() {
    require([ "dojo/sniff"],
    function (sniff) {
        var value = document.body;
        var title = document.title;
        var parent = window;
        var url = parent.location.pathname;
        var filename = url.substring(url.lastIndexOf('/') + 1, url.lastIndexOf('.'));
        while (parent.parent && (parent.parent != parent)) {
            parent = parent.parent;
        }
        if (sniff("ie") < 10)
        var data = "{'id': '" + filename + "', 'name': '" + title + "', 'value': '" + escape(value.innerHTML) + "'}"; else
        var data = {
            'id': filename, 'name': title, 'value': value.innerHTML
        };
        parent.postMessage(data, '*');
    });
}

/*function loadJSON() {
require([ "dojo/sniff"],
function (sniff) {
var value = document.body;
var parent = window;
var url = parent.location.pathname;
var filename = url.substring(url.lastIndexOf('/') + 1, url.lastIndexOf('.'));
while (parent.parent && (parent.parent != parent)) {
parent = parent.parent;
}
var data = typeof (value.innerText) != 'undefined' ? value.innerText: value.textContent;
if (sniff("ie") < 10)
var data = "{'id': '" + filename + "', 'name': '', 'value': '" + escape(JSON.stringify(eval('(' + data + ')'))) + "'}"; else
var data = {'id': filename, 'name': '', 'value': data};
parent.postMessage(data, '*');
});
}*/

function loadTreeInto(target, id, part, action, callback) {
    if (window[id] != null && typeof (window[id]) != 'undefined') {
        loadTree(target, window[id], part, action, callback);
    } else {
        getContentId(id, function (realId) {
            getContent(realId, true, function (json) {
                loadTree(target, json, part, action, callback);
            });
        });
    }
}

function loadTree(target, treeArray, part, action, callback) {
    require([ "dojo/query", "dojo/store/Memory", "dijit/tree/ObjectStoreModel", "dijit/Tree", "dijit/registry", "dojo/domReady!"],
    function (query, Memory, Store, Tree, registry) {
        if (treeArray != null && typeof (treeArray) != 'undefined') {
            var t = registry.byId(target);
            if (t == null || typeof (t) == 'undefined')
            t = query('#' + target)[0];
            
            var myStore = new Memory({ data: treeArray, getChildren: function (item) {
                    var temp = this.query({
                        parent: item.id
                    });
                    return temp;
                }
            });
            var myModel = new Store({
                store: myStore, query: {
                    id: part
                }
            });
            var tree = new Tree({
                model: myModel, onClick: action, id: target + '_tree', showRoot: false, style: 'overflow-x: hidden', autoExpand: true, getDomNodeById: function (id) {
                    return this._itemNodesMap[id][0];
                }
            });
            tree.placeAt(t).startup();
        }
        if (callback != null && typeof (callback) != 'undefined')
        callback();
    });
}

function loadContentInto(target, contentId, callback) {
    require([ "dijit/registry", "dojo/query", "dojo/html", "dojo/_base/fx", "dojo/on", "dojo/dom-style", "dojo/domReady!"],
    function (registry, query, html, fx, on, style) {
        if (contentId.length <= 0) {
            if (callback != null && typeof (callback) != 'undefined')
            callback();
            return;
        }
        var t = registry.byId(target);
        if (t == null || typeof (t) == 'undefined')
        t = query('#' + target)[0];
        var node = t.domNode;
        if (node == null || typeof (node) == 'undefined')
        node = t;
        
        var id = contentId.id
        
        if (id == null || typeof (id) == 'undefined')
        id = contentId;
        var out = fx.fadeOut({
            node: node, duration: 50
        });
        on(out, "End", function () {
            getContentId(id, function (realId) {
                selectNavNode('_nav_tree', realId, function () {
                    getContent(realId, false, function (content) {
                        var t = registry.byId('_htmlHeaderTitle');
                        if (t == null || typeof (t) == 'undefined')
                        t = query('#_htmlHeaderTitle')[0];
                        if (typeof (content.value) != 'undefined') {
                            html.set(node, content.value);
                            html.set(t, content.name);
                        } else
                        html.set(node, content);
                        style.set("contentBg", "background-image", "none");
                        fx.fadeIn({
                            node: node, duration: 50
                        }).play();                        
                        loadRotation(callback);
                    });
                });
            });
        });
        out.play();
        style.set("contentBg", "background-image", "");
    });
}

function selectNavNode(treeId, nodeId, callback) {
    require([ "dijit/registry"],
    function (registry) {
        var tree = registry.byId(treeId);
        if (tree != null && typeof (tree) != 'undefined') {
            if (tree.selectedItem == null || typeof (tree.selectedItem) == 'undefined' || tree.selectedItem.id != nodeId) {
                var node = navigation.index[nodeId];
                var tempNode = node;
                var pathArray =[];
                while (typeof (tempNode.root) == 'undefined' && tempNode.root != true) {
                    pathArray.unshift(tempNode);
                    tempNode = navigation.index[tempNode.parent];
                }
                tree.set('path', pathArray);
            }
        }
        if (callback != null && typeof (callback) != 'undefined')
        callback();
    });
}

function loadIndex(index, callback) {
    require([ "dijit/registry", "dojo/query", "dojo/dom-style", "dojo/window"],
    function (registry, query, style, win) {
        var target = index.target;
        if (target == null || typeof (target) == 'undefined') {
            target = index; 
        } else if (target.length <= 0) {
            if (callback != null && typeof (callback) != 'undefined')
                callback();
            return;
        } else {
            var cb = callback;
            callback = function () {
                var t = registry.byId(target);
                if (t == null || typeof (t) == 'undefined')
                    t = query('#' + target)[0];
                var parent = t.parentNode;
                if (parent == null || typeof (parent) == 'undefined')
                    parent = t.domNode.parentNode;
                style.set(parent, "display", "inline");
                style.set(parent, "visibility", "visible");
                win.scrollIntoView(parent);
                if (cb != null && typeof (cb) != 'undefined')
                    cb();
            };
        }
        loadContentInto('_content', target, callback);
    });
}

function loadLink(target, callback) {
    require([ "dijit/registry", "dojo/query", "dojo/window"],
    function (registry, query, win) {
        var cb = callback;
        callback = function () {
            var t = registry.byId(target);
            if (t == null || typeof (t) == 'undefined')
                t = query('#' + target)[0];
            win.scrollIntoView(t);
            if (cb != null && typeof (cb) != 'undefined')
                cb();
        };
        loadContentInto('_content', target, callback);
    });
}

function markContent(string, callback) {
    require([ "dojo/dom-construct", "dojo/query", "dijit/registry", "dojo/domReady!"],
    function (cst, query, registry) {
        query('.mark').forEach(function (node) {
            var joinedText = "";
            textNodesUnder(node).forEach(function (textNode) {
                joinedText += textNode.data;
            });
            if (node.previousSibling != null && node.previousSibling.nodeType == 3) {
                joinedText = node.previousSibling.data + joinedText;
                cst.destroy(node.previousSibling);
            }
            if (node.nextSibling != null && node.nextSibling.nodeType == 3) {
                joinedText += node.nextSibling.data;
                cst.destroy(node.nextSibling);
            }
            cst.place(document.createTextNode(joinedText), node, "replace");
        });
        if (string.length <= 0) {
            if (callback != null && typeof (callback) != 'undefined')
            callback();
            return;
        }
        var t = registry.byId('_content');
        if (t == null || typeof (t) == 'undefined')
        t = query('#_content')[0];
        markHtml(t, string);
    });
}

function markHtml(node, value, callback) {
    require([ "dojo/dom-construct", "dojo/domReady!"],
    function (cst) {
        value = value.toLowerCase();
        var strings = value.split(' ');
        var colors =[ "#ff7d4a", "#ff6", "#a0ffff", "#9f9", "#f99", "#f6f"];
        var domNode = node.domNode;
        if (domNode == null || typeof (domNode) == 'undefined')
        domNode = node;
        strings.forEach(function (string, id) {
            if (string.length > 0) {
                var textNodes = textNodesUnder(domNode);
                textNodes.forEach(function (textNode) {
                    var curNode = textNode;
                    var oldHtml = textNode.data;
                    var end = 0;
                    var len = string.length;
                    while (end != -1) {
                        end = oldHtml.toLowerCase().indexOf(string, 0);
                        if (end == -1) {
                            cst.place(document.createTextNode(oldHtml), curNode, "after");
                            break;
                        }
                        var tempText = oldHtml.substring(0, end);
                        
                        if (tempText.length > 0)
                        curNode = cst.place(document.createTextNode(tempText), curNode, "after");
                        var stringText = oldHtml.substring(end, end + len);
                        var pos = id % colors.length;
                        //console.log(pos);
                        curNode = cst.place('<span class="mark" style="background-color:' + colors[pos] + ' !important">' + stringText + '</span>', curNode, "after");
                        oldHtml = oldHtml.substring(end + len);
                    }
                    cst.destroy(textNode);
                });
            }
        });
        if (callback != null && typeof (callback) != 'undefined')
        callback();
    });
}

function textNodesUnder(root) {
    var n, a =[];
    if (root != null && typeof (root) != 'undefined') {
        var walk = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, null, false);
        while (n = walk.nextNode()) a.push(n);
    }
    return a;
}

function search(property, callback) {
    require([ "dojo/parser", "dojo/io-query", "dojo/domReady!"],
    function (parser, ioQuery) {
        if (property != null && property.length > 0)
        searchEngine(property, customprop, callback); else
        searchEngine(null, searchtext, callback);
    });
}

function searchEngine(property, sText, callback) {
    require([ "dijit/registry", "dojo/store/Memory", "dijit/tree/ObjectStoreModel", "dijit/Tree", "dojo/dom-construct", "dojo/io-query", "dojo/query", "dijit/form/TextBox", "dojo/domReady!"],
    function (registry, Memory, Store, Tree, cst, ioQuery, query) {
        var uri = window.location.search;
        var objQuery = ioQuery.queryToObject(uri.substring(uri.indexOf("?") + 1, uri.length));
        var partStore = new Memory({
            data: partmap
        });
        var contentStore = new Memory({
            data: contentmap
        });
        var part = loadPart(objQuery, partStore);
        var store = new Memory({
            data: sText
        });
        var results = null;
        var value = null;
        var searchText = null;
        
        if (property != null && property.length > 0) {
            var name = property.split(':')[0];
            value = property.split(':')[1];
            results = store.query(function (item) {
                return item.name == name && item.value == value;
            });
        } else {
            var t = registry.byId('_searchfield');
            if (t == null || typeof (t) == 'undefined')
            t = query('#_searchfield')[0];
            
            searchText = t.get('value').toLowerCase();
            var strings = searchText.split(' ');
            results = store.query(function (item) {
                var haystack = item.value.toLowerCase();
                var ret = false;
                strings.forEach(function (value) {
                    ret = ret || haystack.indexOf(value) !== -1
                });
                return ret;
            });
        }
        var sections = partStore.data[partStore.index[part]].references;
        var sectionNodes = contentStore.query(function (item) {
            return sections.indexOf(item.id) >= 0;
        });
        var hits = results.filter(function (item) {
            if (item.target == part || sections.indexOf(item.target) >= 0)
            return true;
            for (var i = 0; i < sectionNodes.length; i++) {
                if (sectionNodes[i].references.indexOf(item.target) >= 0)
                return true;
            }
        });
        if (property != null && property.length > 0) {
            if (hits.length > 0) {
                loadContentInto('_content', hits[0][ 'target'], callback);
            } else {
                loadContentInto('_content', part, callback);
            }
        } else {
            var treeArray =[ {
                'id': 'root', 'name': 'root', 'root': true
            }];
            var indexes =[];
            var markArray = {
            };
            
            hits.forEach(function (result) {
                var parent = contentStore.query(function (item) {
                    return item.references.indexOf(result[ 'target']) >= 0;
                });
                if (parent.total == 0)
                parent = partStore.query(function (item) {
                    return item.references.indexOf(result[ 'target']) >= 0;
                });
                if (indexes.total == 0 || indexes.indexOf(parent[0][ 'id']) == -1) {
                    treeArray.push({
                        'id': result[ 'target'], 'name': parent[0][ 'name'], 'parent': 'root'
                    });
                    indexes.push(parent[0][ 'id']);
                    markArray[parent[0][ 'id']] = new Array();
                }
                markArray[parent[0][ 'id']].push(result[ 'target']);
            });
            var resultTree = registry.byId('_resultTree');
            if (resultTree == null || typeof (resultTree) == 'undefined')
            resultTree = query('#_resultTree')[0];
            
            store = new Memory({
                data: treeArray, getChildren: function (item) {
                    return this.query({
                        parent: item.id
                    });
                }
            });
            var myModel = new Store({
                store: store, query: {
                    root: true
                }
            });
            var tree = new Tree({
                model: myModel,
                onClick: function (item) {
                    loadContentInto('_content', item, function () {
                        markContent(searchText, callback);
                    });
                },
                showRoot: false,
                style: 'overflow: hidden',
                autoExpand: true
            });
            cst.empty(resultTree);
            tree.placeAt(resultTree).startup();
            
            if (treeArray.length == 2) {
                getContentId(treeArray[1][ 'id'], function (realId) {
                    var t = registry.byId('_content');
                    if (t == null || typeof (t) == 'undefined')
                    t = query('#_content' + target)[0];
                    var domNode = t.domNode;
                    if (domNode == null || typeof (domNode) == 'undefined')
                    domNode = t;
                    if (domNode.firstElementChild != null && typeof (domNode.firstElementChild) != 'undefined') {
                        getContentId(domNode.firstElementChild.id, function (curId) {
                            if (realId != curId) {
                                loadContentInto('_content', treeArray[1], function () {
                                    markContent(searchText, callback);
                                });
                            }
                        });
                    } else {
                        loadContentInto('_content', treeArray[1], function () {
                            markContent(searchText, callback);
                        });
                    }
                });
            } else if (callback != null && typeof (callback) != 'undefined')
            callback();
        }
    });
}