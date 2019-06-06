//// 
//// Listeners
////
var min = 200;
var max = 600;
var mainmin = 200;

window.onerror = function (message, url, lineNumber) {
    ftError(message);
    return true;
};  

$(document)
    .ajaxStart(function () {
        $('#workingDiv').show();
        var n = _getCurrentTime();
        $('#ft-ajax').html("<p>" + n + " - Ajax START</p>");
    })
    .ajaxError(function (event, jqxhr, settings, thrownError) {
        ftError(thrownError);
    })
    .ajaxStop(function () {
        $('#workingDiv').hide();
        var n = _getCurrentTime();
        $('#ft-ajax').html("<p>" + n + " - Ajax END</p>");

        var cols = document.querySelectorAll('.draggable');
        [].forEach.call(cols, function (col) {
            col.addEventListener('dragenter', handleDragEnter, false);
            col.addEventListener('dragleave', handleDragLeave, false);
        });
    });

//$(document).on("keypress", "form", function (event) {
//    return event.keyCode != 13;
//});

$(document).on("keypress", ":input:not(textarea)", function (event) {
    return event.key != "Enter";
})


$(function () {
    $('#jstree_div').bind("select_node.jstree", function (e, data) {
        $('#jstree_div').jstree('save_state');
    });

    $('#jstree_div').on("activate_node.jstree", function (e, data) {
        var link = data.node.a_attr.href;
        var id = data.node.id;
        $.ajax({
            type: "get",
            url: link,
            success: function (d) {
                /* d is the HTML of the returned response */
                $('.sub-container').html(d); //replaces previous HTML with action

                ChangeUrl("Index", link);
                pushHistory("goToFolder(" + id + ", true, false)", true);
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


    var cols = document.querySelectorAll('.draggable');
    [].forEach.call(cols, function (col) {
        col.addEventListener('dragenter', handleDragEnter, false);
        col.addEventListener('dragleave', handleDragLeave, false);
    });

    var i = document.getElementById('breadCont');
    var width = i.scrollWidth;

    $(i).scrollLeft(width);

});

$('#split-bar').mousedown(function (e) {
    e.preventDefault();
    $(document).mousemove(function (e) {
        e.preventDefault();
        var x = e.pageX - $('#sidebar').offset().left;
        if (x > min && x < max && e.pageX < ($(window).width() - mainmin)) {
            $('#sidebar').css("width", x);
            $('#main').css("margin-left", x);
        }
    })
});
$(document).mouseup(function (e) {
    $(document).unbind('mousemove');
});
