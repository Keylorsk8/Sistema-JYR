$(document).ready(function () {
    $(window).scroll(function () {
        if ($(this).scrollTop() > 100) {
            $('.scrollup').fadeIn();
        } else {
            $('.scrollup').fadeOut();
        }
    });
    $('.scrollup').click(function () {
        $("html, body").animate({
            scrollTop: 0
        }, 600);
        return false;
    });
})

$(document).ready(function () {
    $('.scrolldown').click(function () {
        var c = $("#Acercade").offset().left;
        $("html, body").animate({
            scrollTop: c
        }, 600);
        return false;
    });
})


