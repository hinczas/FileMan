////
//// AJAX calls
////
var timer;

async function getNavigationMenu(url) {
    
    await $.ajax({
        url: '/NavigationHistory/NavigationMenu/',
        type: 'get',
        data: {url: url},
        success: function (data) {
            $('#navigationMenu').html(data);
        }
    });
}

function pushHistory(fun, man) {
    var link = "/NavigationHistory/create/";
    var dt = { jsfun: fun, manual: man };
    $.ajax({
        type: "post",
        url: link,
        data: dt,
        success: function (result) {
            if (result.success) {
                if (result.back) {
                    enableBack();
                } else {
                    disableNav('#navBack');
                }
                if (result.forth) {
                    enableForth();
                } else {
                    disableNav('#navForth');
                }
            }
        }
    });
}

function goBack() {
    var link = "/NavigationHistory/GetBackFunction/";
    $.ajax({
        type: "get",
        url: link,
        success: function (res) {
            if (res.success) {
                var x = eval(res.func);
                //enableForth();
                if (res.dis) {
                    disableNav('#navBack');
                }
            } else {
                disableNav('#navBack');
            }
        }
    });
}

function goForth() {

    var link = "/NavigationHistory/GetForthFunction/";
    $.ajax({
        type: "get",
        url: link,
        success: function (res) {
            if (res.success) {
                var x = eval(res.func);
                if (res.dis) {
                    disableNav('#navForth');
                }
                //enableBack();
            } else {
                disableNav('#navForth');
            }
        }
    });
}

function disableNav(btn) {
    $(btn).addClass("disabled");
    $(btn).removeAttr("onclick");
}

function enableForth() {
    $('#navForth').removeClass("disabled");
    $('#navForth').attr("onclick","goForth()");
}

function enableBack() {
    $('#navBack').removeClass("disabled");
    $('#navBack').attr("onclick", "goBack()");
}

function performLogout(form) {
    var link = "/NavigationHistory/ClearHistory/";
    $.ajax({
        type: "post",
        url: link,
        success: function (func) {
            $(form).submit();
        }
    });
}

function addFavourite(itemId, itemType) {
    var link = "/Home/AddFavourite/";
    var dt = { id: itemId, itemType: itemType };
    $.ajax({
        type: "post",
        url: link,
        data: dt,
        success: function (response) {
            if (response.success) {
                ftInfo(response.responseText);
                if (response.itemType==0) {
                    goToFolder(response.itemId);
                }
                if (response.itemType == 1) {
                    goToFile(response.itemId);
                }
                getSidebard();
            } else {
                alert(response.responseText);
                ftError(response.responseText);
            }
        }
    });
}
function delFavourite(itemId, favId) {
    var link = "/Home/DelFavourite/";
    var dt = { id: favId };
    $.ajax({
        type: "post",
        url: link,
        data: dt,
        success: function (response) {
            if (response.success) {
                ftInfo(response.responseText);
                if (response.itemType == 0) {
                    goToFolder(response.itemId);
                }
                if (response.itemType == 1) {
                    goToFile(response.itemId);
                }
                getSidebard();
            } else {
                alert(response.responseText);
                ftError(response.responseText);
            }
        }
    });
}

function getSidebard() {
    var link = "/Home/UserSidebar/";
    $.ajax({
        type: "get",
        url: link,
        success: function (d) {
            /* d is the HTML of the returned response */
            $('#userSidebar').html(d); //replaces previous HTML with action
        }
    });
}

function mainSearch(_form) {
    var link = "/Home/TreeIndex/";
    var dt = $(_form).serialize();
    goToSearch(dt);
    //$.ajax({
    //    type: "get",
    //    url: link,
    //    data: dt,
    //    success: function (d) {
    //        /* d is the HTML of the returned response */
    //        $('.sub-container').html(d); //replaces previous HTML with action
    //        hideNavButton('#navDoc');
    //        hideNavButton('#navCat');
    //        hideNavButton('#navTools');
    //    }
    //});
}
function goToSearch(dat, manual=true) {
    var link = "/Home/TreeIndex/";
    var dt = dat;
    $.ajax({
        type: "get",
        url: link,
        data: dt,
        success: function (d) {
            /* d is the HTML of the returned response */
            $('.sub-container').html(d); //replaces previous HTML with action
            hideNavButton('#navDoc');
            hideNavButton('#navCat');
            pushHistory("goToSearch('" +dat+ "', false)", manual);
        }
    });
}
function renameCategory(_form) {
    var link = "/Folders/Rename/";
    var dt = $(_form).serialize();

    var inp = $('#newCatName').val()
    var name = $.trim(inp) != "";
    if (name != "") {
        $.ajax({
            type: "post",
            url: link,
            data: dt,
            success: function (response) {
                if (response.success) {
                    _hideModal('#renameModal');
                    _renameNode(response.id, response.name)
                    goToFolder(response.parentId);
                    ftInfo(response.responseText);
                } else {
                    alert(response.responseText);
                    ftError(response.responseText);
                }
            }
        });
    }
}

function scrollBread() {
    var i = document.getElementById('breadCont');
    var width = i.scrollWidth;

    $(i).scrollLeft(width);
}

function goToFolder(id, redirect = true, manual = true) {
    var link = "/Home/TreeIndex/"+id;
    $.ajax({
        type: "get",
        url: link,
        success: function (d) {
            /* d is the HTML of the returned response */
            $('.sub-container').html(d); //replaces previous HTML with action
            if (redirect) {
                if (id == null) {
                    deselectTree();
                } else {
                    //activateNode(id);
                    selectNode(id);
                }
                ChangeUrl("Index", link);
            }
            showNavButton('#navDoc');
            showNavButton('#navCat');
            scrollBread();
            pushHistory("goToFolder(" + id + ", " + redirect +", false)", manual);
        }
    });
}

function lockDocument(id, doLock, pid) {
    var link = "/MasterFiles/UserLock/";
    var dt = { id: id, isLocked: doLock };
    $.ajax({
        type: "post",
        url: link,
        data: dt,
        success: function (response) {
            if (response.success) {
                ftInfo(response.responseText);
                goToFile(id, pid);
                getSidebard();
            } else {
                alert(response.responseText);
                ftError(response.responseText);
            }
        }
    });
}

function goToFile(id, pid = null, manual = true) {
    var link = "/MasterFiles/PartialDetails/";
    if (pid == null) {
        pid = -1;
    }
    $.ajax({
        type: "get",
        url: link,
        data: { id: id, pid: pid },
        success: function (d) {
            /* d is the HTML of the returned response */
            $('.sub-container').html(d); //replaces previous HTML with action
            link = link.replace("PartialDetails","Details")
            ChangeUrl("Details", link);
            hideNavButton('#navDoc');
            hideNavButton('#navCat');

            pushHistory("goToFile(" + id + ", " + pid + ", false)", manual);
        }
    });
}

function goToEditFile(id, pid = null, manual = true) {
    var link = "/MasterFiles/PartialEdit/";
    if (pid == null) {
        pid = -1;
    }
    $.ajax({
        type: "get",
        url: link,
        data: { id: id, pid: pid },
        success: function (d) {
            /* d is the HTML of the returned response */
            $('.sub-container').html(d); //replaces previous HTML with action
            link = link.replace("PartialEdit", "Edit")
            ChangeUrl("Edit", link);
            hideNavButton('#navDoc');
            hideNavButton('#navCat');
            pushHistory("goToEditFile(" + id + ", " + pid + ", false)", manual);
        }
    });
}

function deleteCategory(id, redirect = false) {
    //var confirmed = confirm('Are you sure you wish to delete this Category?');
    var confirmed = true;
    if (confirmed) {
        $.ajax({
            url: '/Folders/Delete/' + id,
            type: 'post',
            success: function (response) {
                if (response.success) {
                    _hideModal('#deleteModal');
                    if (redirect) {
                        goToFolder(response.parentId);
                    } else {
                        addTableRow(response.parentId);
                    }
                    removeNode(id);
                    ftInfo(response.responseText);
                } else {
                    alert(response.responseText);
                    ftError(response.responseText);
                }
            }
        });
    }
}

function createCategory(_form, _url, _pn) {

    var inp = $('#catNameInp').val()
    var name = $.trim(inp) != "";

    if (name) {
        var dt = $(_form).serialize();

        $.ajax({
            url: _url,
            type: 'post',
            data: dt,
            success: function (response) {
                if (response.success) {
                    //addNode(response.parentId, response.id, response.name);
                    addMultipleNodes(response.folders, response.parentId);
                    addTableRow(response.parentId);
                    $(_form).trigger("reset");
                    ftInfo(response.responseText);
                } else {
                    alert(response.responseText);
                    ftError(response.responseText);
                }
            }
        });
    }
}

function addMultipleNodes(list, pid) {
    if (list !== "undefined" && list.length > 0) {
        $.each(list, function (k, v) {
            addNode(pid, v.Id, v.Name);
        });
    }
}

async function deleteDocument(did, pid) {
    var confirmed = confirm('Are you sure you wish to delete this Document?');

    if (confirmed) {
        await $.ajax({
            url: '/MasterFiles/DeleteAsync',
            type: 'post',
            data: { id: did, folderId: pid },
            contentType: "application/json; charset=utf-8",
            //data: {'id': did, 'parentId': pid},
            success: function (response) {
                if (response.success) {
                    if (pid > -1) {
                        goToFolder(pid, false);
                    } else {
                        goToFile(did);
                    }
                    //_updateNodeQte(pid);
                    ftInfo(response.responseText);
                } else {
                    alert(response.responseText);
                    ftError(response.responseText);
                }
            }
        });
    }
}

function createDocument(_form, _pid) {
    var dt = $(_form).serialize();
    $.ajax({
        url: '/MasterFiles/Create/',
        type: 'post',
        data: dt,
        success: function (response) {
            if (response.success) {
                _hideModal('#filModal');
                goToFolder(_pid, false);
                _updateNodeQte(_pid);
                ftInfo(response.responseText);
            } else {
                alert(response.responseText);
                ftError(response.responseText);
            }
        }
    });
}

function editDocument(_form, _pid) {
    var dt = $(_form).serialize();
    $.ajax({
        url: '/MasterFiles/Edit/',
        type: 'post',
        data: dt,
        success: function (response) {
            if (response.success) {
                goToFile(response.id, response.parentId);
                ftInfo(response.responseText);
            } else {
                alert(response.responseText);
                ftError(response.responseText);
            }
        }
    });
}

function moveDocument(_form) {
    var dt = $(_form).serialize();
    $.ajax({
        url: '/MasterFiles/MoveFile/',
        type: 'post',
        data: dt,
        success: function (response) {
            if (response.success) {
                _hideModal('#movModal');
                goToFile(response.id, response.parentId);
                _updateNodes(response.affFolIds);
                ftInfo(response.responseText);
            } else {
                alert(response.responseText);
                ftError(response.responseText);
            }
        }
    });
}

function changePassword(_form) {
    var dt = $(_form).serialize();
    $.ajax({
        url: '/Manage/ChangePassword/',
        type: 'post',
        data: dt,
        success: function (response) {
            if (response.success) {
                _hideModal('#passwordModal');
                ftInfo(response.responseText);
            } else {
                alert(response.responseText);
                ftError(response.responseText);
            }
        }
    });
}

function moveDnDDocument(id, opid, npid) {
    $.ajax({
        url: '/MasterFiles/MoveFileAsync/',
        type: 'post',
        data: {id: id, opid: opid, npid: npid},
        success: function (response) {
            if (response.success) {
                _updateNodes(response.affFolIds);
                ftInfo(response.responseText);
            } else {
                alert(response.responseText);
                ftError(response.responseText);
            }
        }
    });
}

function moveDocuments(_form) {
    var form = $(_form)[0];
    var dt = $(form).serialize();

    var inps = $("input[name='files']:checked");

    if (inps.length > 0) {
        $.ajax({
            url: '/Folders/MoveFiles/',
            type: 'post',
            data: dt,
            success: function (response) {
                if (response.success) {
                    goToFolder(response.parentId, false);
                    _updateNodeQte(response.parentId);
                    ftInfo(response.responseText);
                } else {
                    alert(response.responseText);
                    ftError(response.responseText);
                }
            }
        });
    }
}

function addDraft(_form) {
    var form = $(_form)[0];
    var selectedFile = ($("#revFileUpl"))[0].files[0];

    var dt = $(_form).serialize();
    var dataString = new FormData(form);
    dataString.append("file", selectedFile);

    $.ajax({
        url: '/FileRevisions/Create/',
        type: 'post',
        contentType: false,
        processData: false,
        data: dataString,
        xhr: function () {  // Custom XMLHttpRequest
            var myXhr = $.ajaxSettings.xhr();
            if (myXhr.upload) { // Check if upload property exists
                // Progress code if you want
            }
            return myXhr;
        },
        success: function (response) {
            if (response.success) {
                _hideModal('#revModal');
                goToFile(response.id, response.parentId);
                ftInfo(response.responseText);
            } else {
                alert(response.responseText);
                ftError(response.responseText);
            }
        }
    });
}

function uncategorise(_id, _pid) {
    var confirmed = confirm('Are you sure you wish to uncategorise this Document?');
    if (confirmed) {
        $.ajax({
            url: '/MasterFiles/MoveFile/',
            type: 'post',
            data: { Id: _id, folders: [], pid: _pid },
            success: function (response) {
                if (response.success) {
                    _hideModal('#movModal');
                    goToFile(response.id, response.parentId);
                    _updateNodes(response.affFolIds);
                    ftInfo(response.responseText);
                } else {
                    alert(response.responseText);
                    ftError(response.responseText);
                }
            }
        });
    }
}

function promote(_form) {
    var dt = $(_form).serialize();
    $.ajax({
        url: '/MasterFiles/Promote/',
        type: 'post',
        data: dt,
        success: function (response) {
            if (response.success) {
                _hideModal('#promModal');
                goToFile(response.id, response.parentId);
                ftInfo(response.responseText);
            } else {
                alert(response.responseText);
                ftError(response.responseText);
            }
        }
    });
}

function editDraft(_form) {
    var dt = $(_form).serialize();
    $.ajax({
        url: '/FileRevisions/EditDraftCommentAsync/',
        type: 'post',
        data: dt,
        success: function (response) {
            if (response.success) {
                _hideModal('#editRevModal');
                goToFile(response.id, response.parentId);
                ftInfo(response.responseText);
            } else {
                alert(response.responseText);
                ftError(response.responseText);
            }
        }
    });
}

function addTableRow(id) {
    var link = "/Home/GetDocTable/" + id;
    $.ajax({
        type: "get",
        url: link,
        success: function (d) {
            /* d is the HTML of the returned response */
            $('#replaceDocTable').html(d); //replaces previous HTML with action
        }
    });
}

function goToManage(id, manual = true) {
    var link = "/Manage/PartialIndex/";
    $.ajax({
        type: "get",
        url: link,
        success: function (d) {
            /* d is the HTML of the returned response */
            $('.sub-container').html(d); //replaces previous HTML with action
            hideNavButton('#navDoc');
            hideNavButton('#navCat');
            hideNavButton('#navTools');
            pushHistory("goToManage(" + id + ", false)", manual);
        }
    });
}


function saveSettings(_form) {
    var dt = $(_form).serialize();

    $.ajax({
        url: "/Manage/SaveSettings/",
        type: 'post',
        data: dt,
        success: function (response) {
            if (response.success) {
                //alert(response.responseText);
                ftInfo(response.responseText);
                //if (response.reload) {
                //    location.reload();
                //}
            } else {
                alert(response.responseText);
                ftError(response.responseText);
                goToManage(null);
            }
        }
    });
}


function enterSubmit(e, _button) {
    //var e = $(_form).find('input');
    // Enter pressed?
    if (e.which == 10 || e.which == 13) {
        $(_button).trigger("click");
        //this.form.submit();
    }
}

function collapseAll() {
    $('#jstree_div').jstree('close_all');
}

function expandAll() {
    $('#jstree_div').jstree('open_all');
}

function activateNode(id) {
    $('#jstree_div').jstree('activate_node', id);
    $('#jstree_div').jstree(true).get_node(id, true).children('.jstree-anchor').focus();
}

function selectNode(id) {
    deselectTree();
    $('#jstree_div').jstree('select_node', id);
}

function EraseUrl() {
    if (typeof (history.pushState) != "undefined") {
        var newUrl = "/";
        var obj = { Page: "Home", Url: newUrl };
        history.pushState(obj, obj.Page, obj.Url);
    }
}

function ChangeUrl(page, url) {
    if (typeof (history.pushState) != "undefined") {
        var newUrl = url.replace("TreeIndex", "Index");
        var obj = { Page: page, Url: newUrl };
        //history.pushState(obj, obj.Page, obj.Url);
    }
}

$(function () {
    $('[data-toggle="tooltip"]').tooltip()
})

function searchMain(e) {
    var value = $(e).val().toLowerCase();

    $("#mainTable tr").filter(function () {
        $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
    });
}

function fileSelector() {
    $("#fileImport").trigger("click");
}

function searchUnassigned(e) {
    var value = $(e).val().toLowerCase();
    $("#tbl-unassigned tbody tr").filter(function () {
        $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
    });
}

function searchMove(e) {
    var value = $(e).val().toLowerCase();
    $("#moveTable tr").filter(function () {
        $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
    });
}

function checkAllItems(e) {
    var checked = $(e).prop('checked');
    $('.form-check-input:visible').each(function () {
        $(this).prop('checked', checked);
    })
}

function _getCurrentTime() {
    var d = new Date();
    var n = d.toLocaleTimeString();

    return n;
}

function ftError(e) {
    var n = _getCurrentTime();
    $('#ft-error').html(n+" - "+e);
}
function ftInfo(e) {
    var n = _getCurrentTime();
    $('#ft-info').html(n + " - " + e);
}
function ftMessage(e) {
    var n = _getCurrentTime();
    $('#ft-msg').html(n + " - " + e);
}

function removeNode(id) {
    $("#jstree_div").jstree('delete_node', id);
}

function addNode(pid, id, name) {
    var position = 'inside';
    //var parent = $('#jstree_div').jstree('get_selected');
    var parent = $('#jstree_div').jstree(true).get_node(pid)
    var newNode = {
        'id': id,
        'text': name,
        'type': 'valid_child',
        'icon': 'fas fa-folder',
        'state': {
            'opened': true
        },
        'a_attr': {
            'href': '/Home/TreeIndex/' + id
        }
    };

    $('#jstree_div').jstree('create_node', parent, newNode, 'last', false, false);
    $('#jstree_div').jstree('open_node', parent);
}

function deselectTree() {
    $('#jstree_div').jstree("deselect_all");
}

$('#jstree_div').on("move_node.jstree", function (e, data) {

    //var moveitemID = $('#' + data.node.id).find('a')[0].id;
    //item being moved                      
    var moveitemID = data.node.id;

    //new parent
    var newParentID = data.parent;

    //old parent
    var oldParentID = data.old_parent;

    var pid = $('#currentPid').val();

    var link = "/Folders/Move/";
    $.ajax({
        url: link,
        type: 'post',
        data: { Id: moveitemID, OldParId: oldParentID, NewParId: newParentID },
        success: function (response) {
            if (response.success) {
                ftInfo(response.responseText);
                if (pid == newParentID || pid == oldParentID) {
                    addTableRow(pid);
                }
            } else {
                ftError(response.responseText);
            }
        }
    });
});

function moveCategory(id, opid, npid) {
    var link = "/Folders/Move/";
    $.ajax({
        url: link,
        type: 'post',
        data: { Id: id, OldParId: opid, NewParId: npid },
        success: function (response) {
            if (response.success) {
                ftInfo(response.responseText);
                moveNode(id, npid);
            } else {
                ftError(response.responseText);
            }
        }
    });
}

function moveNode(id, pid) {
    $("#jstree_div").jstree("move_node", id, pid, 0);
}

function _hideModal(name) {
    $(name).modal('hide');
    $('body').removeClass('modal-open');
    $('.modal-backdrop').remove();
}

function _updateNodes(nodes) {
    if (nodes.length > 0) {
        $.each(nodes, function(i, v) {
                _updateNodeQte(v);
            }
        );
    }
}
function _updateNodeQte(id) {
    $.ajax({
        url: '/Folders/GetChildCount/'+id,
        type: 'get',
        success: function (ret) {
            //var node = $('#jstree_div').jstree(true).get_node(id)
            //var parent = node.parent;
            //node.a_attr.data_quantity = ret;
            ////$("#jstree_div").jstree(true).refresh_node(node);
            var node = $('#jstree_div').jstree('get_node', id);
            $('#jstree_div').jstree('get_node', id).a_attr['data_quantity'] = ret;
            $('#jstree_div').jstree(true).redraw(true);
            //$('#jstree_div').jstree(true).refresh();
            //var nid = "#"+id+"_anchor";
            //$(nid).attr("data_quantity", ret);
        }
    });
}
function _renameNode(id, name) {
    $("#jstree_div").jstree('rename_node', id, name);
    //var nid = "#" + id + "_anchor";
   // $(nid).text('<i class="jstree-icon jstree-themeicon fas fa-folder jstree-themeicon-custom" role="presentation"></i>'+name);
}

function refreshTree() {
    var id = $("#jstree_div").jstree("get_selected");
    if (id.length < 1) {
        id = -1;
    } else {
        id = id[0];
    }
    var url = "/Home/GetTree?id=" + id;
    $.get(url,
        function (data) {
            $('#jstree_div').jstree(true).settings.core.data = data;
            $('#jstree_div').jstree(true).refresh();
            $('#jstree_div').jstree(true).redraw(true);
        }
    )
}

/* DRAG 'n' DROP */
function allowDrop(ev) {
    ev.preventDefault();
    
}

function drag(ev) {
    ev.dataTransfer.setData("text", ev.target.id);
    ev.dataTransfer.effectAllowed = "copy";
}

function drop(ev) {
    ev.preventDefault();
    var sourceId = ev.dataTransfer.getData("text");
    var doc = sourceId.startsWith("doc_");
    var npid = ev.currentTarget.id.replace("cat_", "");
    var pid = $('#currentPid').val();

    if (doc) {
        var id = sourceId.replace("doc_", "");
        moveDnDDocument(id, pid, npid);
        goToFolder(pid, false);
    } else {
        var id = sourceId.replace("cat_", "");
        moveNode(id, npid);
        addTableRow(pid);
    }
}

function handleDragEnter(e) {
    this.classList.add('over');  
}

function handleDragLeave(e) {
    this.classList.remove('over');
    clearTimeout(timer);
}

function mouseEnterHandler(e) {
    var that = e;
    timer = setTimeout(function () {
        $(that).css('cursor', 'pointer');
        $(that).on('click', function () {
            var func = $(that).attr("func");

            var x = eval(func);
        });
    }, 2000);
}

function mouseLeaveHandler(e) {
    $(e).off('click');
    $(e).css('cursor', 'not-allowed');
    clearTimeout(timer);
}


function copyToClip(element) {
    /* Get the text field */
    var copyText = $(element);

    /* Select the text field */
    copyText.focus();
    copyText.select();

    /* Copy the text inside the text field */
    document.execCommand("copy");
}

function hideNavButton(navId) {
    $(navId).addClass('disabled');
    $(navId).removeAttr('data-toggle');
}

function showNavButton(navId) {
    $(navId).removeClass('disabled');
    var ex = $(navId).data("extra");
    $(navId).attr("data-toggle", ex);
}
