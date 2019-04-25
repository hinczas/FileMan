function goToFolder(id) {
    var link = "/Home/TreeIndex/"+id;
    $.ajax({
        type: "get",
        url: link,
        success: function (d) {
            /* d is the HTML of the returned response */
            $('.sub-container').html(d); //replaces previous HTML with action
            activateNode(id);
            ChangeUrl("Index", link);
        }
    });
}

function goToFile(id) {
    var link = "/MasterFiles/PartialDetails/" + id;
    $.ajax({
        type: "get",
        url: link,
        success: function (d) {
            /* d is the HTML of the returned response */
            $('.sub-container').html(d); //replaces previous HTML with action
            link = link.replace("PartialDetails","Details")
            ChangeUrl("Details", link);
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
        history.pushState(obj, obj.Page, obj.Url);
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

function submitForm(_form, _url, _pn) {
    var dt = $(_form).serialize();
    $.ajax({
        url: _url,
        type: 'post',
        data: dt,
        success: function (response) {
            if (response.success) {
                addNode(response.parentId, response.id, response.name);
                addTableRow(response.parentId);
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