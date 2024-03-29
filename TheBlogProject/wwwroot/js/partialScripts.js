﻿var BlockNumber = 1;
var noMoreData = false;
var text = $(".postPartialContainer").attr('id');
var tag = $(".postPartialContainer").attr('alt');
var inProgress = false;


$(window).ready(function () {
    console.log("doc rdy");
    $("#progress").hide();

    $(".tagsManModalBtn").off("click").on('click', function () {
        $.ajax({
            type: 'GET',
            url: "/Home/TagManagement",
            dataType: 'html',
            success: function (data) {

                $(".modal-body").html("");
                $(".modal-body").append(data);
            },

            error: function () {
                alert("Error while retrieving data!");
            }
        });
    })

    $(".tagsModalBtn").off("click").on('click', function () {

        $.ajax({
            type: 'GET',
            url: "/Home/UserTags",
            dataType: 'html',
            success: function (data) {

                $(".modal-body").html("");
                $(".modal-body").append(data);
            },

            error: function () {
                alert("Error while retrieving data!");
            }
        });
    })

    if (inProgress == true) {
        return;
    }
    inProgress = true;

    if (noMoreData == true) {
        return;
    }

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
        statusCode: {
            500: function () {
                GetData();
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
    

});

$(window).on("scroll", function () {
    var docHeight = $(document).height()-500;
    var winScrolled = $(window).height() + $(window).scrollTop();
    if ((docHeight - winScrolled) < 1 && !noMoreData) {
        GetData();
    }
})



function GetData() {

    if (inProgress == true) {
        return;
    }
    inProgress = true;

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

    $(".likeBtn").off("click").on('click', function () {
        var $el = $(this);
        var slug = $el.data('slug');
        var postLikesCount = $(this).attr('postLikesCount');

        var e = $el.parents("#viewUpdate").children().children();
        var elId = e.attr('id');
        var pageCount = $("#" + elId).children(".spanPostPartial").children().html();
        console.log(pageCount);
        postLikesCount = pageCount;

        let modalId = $(this).attr('id');

        $.ajax({

            type: "POST",
            url: '/Home/Like',
            data: { slug: slug },
            dataType: "json",
            success: function (result) {
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
                console.log(e.attr('id'));
                var elId = e.attr('id');


                $("#" + elId).children(".spanPostPartial").children(".postSpan").html(" " + postLikesCount);


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
        var pageCount = $("#" + elId).children(".spanPostPartial").children().html();
        postUsefulCount = pageCount;

        let modalId = $(this).attr('id');

        $.ajax({

            type: "POST",
            url: '/Home/UsefulCode',
            data: { slug: slug, postUsefulCount: postUsefulCount },
            dataType: "json",
            success: function (result) {

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

                $("#" + elId).children(".spanPostPartial").children(".postSpan").html(" " + postUsefulCount);


            },
            error: function (req, status, error) {
                console.log(status)
            }
        });
    });

}