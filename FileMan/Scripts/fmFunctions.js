////
//// AJAX calls
////
function mainSearch(_form) {
    var link = "/Home/TreeIndex/";
    var dt = $(_form).serialize();
    $.ajax({
        type: "get",
        url: link,
        data: dt,
        success: function (d) {
            /* d is the HTML of the returned response */
            $('.sub-container').html(d); //replaces previous HTML with action
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
                } else {
                    alert(response.responseText);
                }
            }
        });
    }
}

function goToFolder(id, redirect = true) {
    var link = "/Home/TreeIndex/"+id;
    $.ajax({
        type: "get",
        url: link,
        success: function (d) {
            /* d is the HTML of the returned response */
            $('.sub-container').html(d); //replaces previous HTML with action
            if (redirect) {
                activateNode(id);
                ChangeUrl("Index", link);
            }
        }
    });
}

function goToFile(id, pid=null) {
    var link = "/MasterFiles/PartialDetails/";
    $.ajax({
        type: "get",
        url: link,
        data: {id: id, pid: pid},
        success: function (d) {
            /* d is the HTML of the returned response */
            $('.sub-container').html(d); //replaces previous HTML with action
            link = link.replace("PartialDetails","Details")
            ChangeUrl("Details", link);
        }
    });
}

function goToEditFile(id, pid = null) {
    var link = "/MasterFiles/PartialEdit/";
    $.ajax({
        type: "get",
        url: link,
        data: { id: id, pid: pid },
        success: function (d) {
            /* d is the HTML of the returned response */
            $('.sub-container').html(d); //replaces previous HTML with action
            link = link.replace("PartialEdit", "Edit")
            ChangeUrl("Edit", link);
        }
    });
}

function deleteCategory(id) {
    var confirmed = confirm('Are you sure you wish to delete this Category?');

    if (confirmed) {
        $.ajax({
            url: '/Folders/Delete/' + id,
            type: 'post',
            success: function (response) {
                if (response.success) {
                    addTableRow(response.parentId);
                    removeNode(id);
                } else {
                    alert(response.responseText);
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
                } else {
                    alert(response.responseText);
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
                } else {
                    alert(response.responseText);
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
            } else {
                alert(response.responseText);
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
            } else {
                alert(response.responseText);
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
            } else {
                alert(response.responseText);
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
                } else {
                    alert(response.responseText);
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
            } else {
                alert(response.responseText);
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
                } else {
                    alert(response.responseText);
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
            } else {
                alert(response.responseText);
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
    $('#jstree_div').jstree('select_node', id);
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


$('#jstree_div').on("move_node.jstree", function (e, data) {

    //var moveitemID = $('#' + data.node.id).find('a')[0].id;
    //item being moved                      
    var moveitemID = data.node.id;

    //new parent
    var newParentID = data.parent;

    //old parent
    var oldParentID = data.old_parent;

    var link = "/Folders/Move/";
    //$.post(link, { Id: moveitemID, OldParId: oldParentID, NewParId: newParentID },
    //    function (returnedData) {
    //        console.log(returnedData);
    //    });
    $('#loadingDiv').show();
    $.ajax({
        url: link,
        type: 'post',
        data: { Id: moveitemID, OldParId: oldParentID, NewParId: newParentID },
        success: function () {
            //alert("done");
            $('#loadingDiv').hide();
        }
    });
});

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