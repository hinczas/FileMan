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

function deleteCategory(id) {
    var confirmed = confirm('Are you sure you wish to delete this Category?');

    if (confirmed) {
        $.ajax({
            url: '/Folders/Delete/'+id,
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
function createCategory(_form, _url, _pn) {
    var dt = $(_form).serialize();
    $.ajax({
        url: _url,
        type: 'post',
        data: dt,
        success: function (response) {
            if (response.success) {
                addNode(response.parentId, response.id, response.name);
                addTableRow(response.parentId);
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

function _updateNodeQte(id) {
    $.ajax({
        url: '/Folders/GetChildCount/'+id,
        type: 'get',
        success: function (ret) {
            //var node = $('#jstree_div').jstree(true).get_node(id)
            //var parent = node.parent;
            //node.a_attr.data_quantity = ret;
            //$("#jstree_div").jstree(true).refresh_node(node);
            var nid = "#"+id+"_anchor";
            $(nid).attr("data_quantity", ret);
        }
    });

}