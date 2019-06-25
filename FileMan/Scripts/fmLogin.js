$(document)
    .ready(async function () {
        await $.ajax({
            url: '/Account/GetBackground/',
            type: 'get',
            //data: {'id': did, 'parentId': pid},
            success: function (response) {
                if (response.success) {
                    $('.main-body').css("background-image", "url(" + response.image + ")");
                    $('#cpLink').html(response.cpRight);
                    $('#cpLink').attr('href', response.cpLink);
                } else {
                    $('.main-body').css("background-image", "url('../../Content/Images/login_img.jpg')");
                }
            },
            error: function () {
                $('.main-body').css("background-image", "url('../../Content/Images/login_img.jpg')");
            }
        });
    });


function submitExternalForm(e) {
    var form = $('#externalForm');
    $(form).submit();
}