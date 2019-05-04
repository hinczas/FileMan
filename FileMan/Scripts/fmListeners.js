//// 
//// Listeners
////

$(document)
    .ajaxStart(function () {
        $('#workingDiv').show();
    })
    .ajaxStop(function () {
        $('#workingDiv').hide();
    });

$(document).on("keypress", "form", function (event) {
    return event.keyCode != 13;
});


$(function () {
    $('#jstree_div').bind("select_node.jstree", function (e, data) {
        $('#jstree_div').jstree('save_state');
    });

    $('#jstree_div').on("activate_node.jstree", function (e, data) {
        //window.location.href = data.node.a_attr.href;

        var link = data.node.a_attr.href;
        $.ajax({
            type: "get",
            url: link,
            success: function (d) {
                /* d is the HTML of the returned response */
                $('.sub-container').html(d); //replaces previous HTML with action

                ChangeUrl("Index", link);
            }
        });
    });


    $('#forceDeleteCheck').change(function () {
        if ($(this).prop('checked')) {
            alert("Warning!\nWhen deleting categories, all sub-categories will be deleted as well.\nAll recursive documents will be uncategorised!"); //checked
        }
        else {
            alert("Info\nOnly empty Categories cn be deleted."); //not checked
        }
    });

    var to = false;
    $('#jstree_div_q').keyup(function () {
        if (to) { clearTimeout(to); }
        to = setTimeout(function () {
            var v = $('#jstree_div_q').val();
            $('#jstree_div').jstree(true).search(v);
        }, 250);
    });
});

$(document).ready(function () {
});