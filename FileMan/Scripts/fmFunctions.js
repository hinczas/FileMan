﻿function goToFolder(id) {
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
    $.ajax({
        url: _url,
        type: 'post',
        data: $(_form).serialize(),
        success: function () {
            addNode();
        }
    });
}

function addNode() {
    var position = 'inside';
    var parent = $('#jstree_div').jstree('get_selected');
    var newNode = {
        'id': 'testrrr',
        'text': 'testrrr',
        'type': 'valid_child',
        'state': {
            'opened': true
        }
    };

    $('#jstree_div').jstree('create_node', parent, newNode, 'last', false, false);
    $('#jstree_div').jstree('open_node', parent);
}