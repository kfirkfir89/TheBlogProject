var BlockNumber = 1;
var noMoreData = false;
var text = $(".postPartialContainer").attr('id');
var tag = $(".postPartialContainer").attr('alt');
var inProgress = false;


$(window).ready(function () {
    console.log("doc rdy");
    $("#progress").hide();
    console.log(tag);
    console.log(text);

    $.ajax({
        type: 'POST',
        url: '/Home/InfinateScroll',
        data: { "BlockNumber": BlockNumber, "text": text, "tag": tag},
        dataType: 'HTML',
        success: function (data) {

            if (data == "true") {

                noMoreData = true;
                $("#progress").hide();
                inProgress = false;
                return;
            }

            if (data != null) {

                $(".postPartialContainer").append(data);
                BlockNumber++;
                inProgress = false;

            }
            GetData();
        },
        beforeSend: function () {
            $("#progress").show();
        },
        complete: function () {
            $("#progress").hide();
        },
        error: function () {
            alert("Error while retrieving data!");
        }
    });
    

});

$(window).on("scroll", function () {
    var docHeight = $(document).height()-500;
    var winScrolled = $(window).height() + $(window).scrollTop();
    if ((docHeight - winScrolled) < 1 && !noMoreData) {
        console.log("module scrolled to bottom");
        console.log(noMoreData);
        GetData();
    }
})



function GetData() {
    console.log("---------------------------");
    console.log("proIN:" + inProgress);
    if (inProgress == true) {
        return;
    }
    inProgress = true;
    console.log("pro:" + inProgress);
    console.log("nomore:" + noMoreData);
    console.log("block:" + BlockNumber);
    if (noMoreData == true) {
        return;
    }

    $.ajax({
        type: 'POST',
        url: '/Home/InfinateScroll',
        data: { "BlockNumber": BlockNumber, "text": text},
        dataType: 'HTML',
        success: function (data) {

            if (data == "true") {

                noMoreData = true;
                $("#progress").hide();
                inProgress = false;
                return;
            }

            if (data != null) {

                $(".postPartialContainer").append(data);
                BlockNumber++;
                inProgress = false;

            }
        },
        beforeSend: function () {
            $("#progress").show();
        },
        complete: function () {
            $("#progress").hide();
        },
        error: function () {
            alert("Error while retrieving data!");
        }
    });
}


function myPartialView_Load() {




    $(".detailsBtn").off("click").on('click', function () {
        console.log("----------postD-----------------");
        var $el = $(this);
        var route = $el.data('route');
        var returnUrl = "~/"
        console.log(route);

        $.ajax({
            type: 'POST',
            url: route,
            data: { slug: returnUrl },
            dataType: 'json',
            success: function (data) {
                console.log(data);
                $(".modal-body").html(data);
            },

            error: function () {
                alert("Error while retrieving data!");
            }
        });
    })

    $(".likeBtn").off("click").on('click', function () {
        var $el = $(this);
        var slug = $el.data('slug');
        var postLikesCount = $(this).attr('postLikesCount');

        var e = $el.parents("#viewUpdate").children().children();
        var elId = e.attr('id');
        var pageCount = $("#" + elId).children().html();
        postLikesCount = pageCount;

        let modalId = $(this).attr('id');

        console.log($el.data('slug'));
        console.log("ziki zuki `` ose string" + ``);
        $.ajax({

            type: "POST",
            url: '/Home/Like',
            data: { slug: slug },
            dataType: "json",
            success: function (result) {
                console.log(result);
                trigger = false;
                var modalAnchor = document.getElementById(modalId);
                var icon = modalAnchor.firstElementChild;

                if (icon.classList.contains("fa-regular")) {
                    icon.classList.remove("fa-regular");
                    icon.classList.add("fa-solid");

                    postLikesCount++;

                } else if (icon.classList.contains("fa-solid")) {
                    icon.classList.remove("fa-solid");
                    icon.classList.add("fa-regular");

                    postLikesCount--;
                }

                var e = $el.parents("#viewUpdate").children().children();
                var elId = e.attr('id');
                console.log(elId);
                $("#" + elId).children().html(" " + postLikesCount);

            },
            error: function (req, status, error) {
                console.log(status)
            }
        });
    });


    $(".usefulBtn").off("click").on('click', function () {

        var $el = $(this);
        var slug = $el.data('slug');
        var postUsefulCount = $(this).attr('postUsefulCount');

        var e = $el.parents("#viewUpdate").children().children(".usefulCount");
        var elId = e.attr('id');
        var pageCount = $("#" + elId).children(".usefulCount").html();
        postUsefulCount = pageCount;

        let modalId = $(this).attr('id');

        console.log($el.data('slug'));
        console.log("ziki zuki `` ose string" + ``);

        $.ajax({

            type: "POST",
            url: '/Home/UsefulCode',
            data: { slug: slug, postUsefulCount: postUsefulCount },
            dataType: "json",
            success: function (result) {
                console.log(result);
                var modalAnchor = document.getElementById(modalId);
                var icon = modalAnchor.firstElementChild;

                if (icon.classList.contains("fa-regular")) {
                    icon.classList.remove("fa-regular");
                    icon.classList.add("fa-solid");

                    postUsefulCount++;

                } else if (icon.classList.contains("fa-solid")) {
                    icon.classList.remove("fa-solid");
                    icon.classList.add("fa-regular");

                    postUsefulCount--;
                }

                var e = $el.parents("#viewUpdate").children().children(".usefulCount");
                var elId = e.attr('id');

                console.log(elId);
                $("#" + elId).children(".usefulCount").html(" " + postUsefulCount);


            },
            error: function (req, status, error) {
                console.log(status)
            }
        });
    });

}