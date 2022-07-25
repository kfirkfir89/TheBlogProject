console.log("work");
console.log("work");
console.log("work");

var NoMoreData = false;
var inProgress = false;


$(document).ready(function () {

    console.log("rdy");

    $(".likeBtn").click(function () {

        var blockId = $(this).parents(".partBlock").attr('id');
        var scriptId = $(this).parents(".partBlock").children(".partScript").attr('id');
        var lastCon = $(this).parents(".lastGetContainer");
        var lastConBlockId = lastCon.children(".partBlock").attr('id');
        var lastConScriptId = lastCon.children(".partBlock").children(".partScript").attr('id');

        console.log(lastCon.attr('id'));
        console.log("Conblock:" + lastConBlockId);
        console.log("block:" + blockId);
        console.log("script:" + lastConScriptId);

        if (lastCon != undefined) {
            console.log("ze ze");
            
        } else {
            console.log("ze lo ze");
            return;
        }


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


    $(".usefulBtn").click(function () {

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

    console.log("rdy");


});