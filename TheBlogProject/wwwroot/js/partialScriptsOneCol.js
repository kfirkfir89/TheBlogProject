var BlockNumber = 1;
var text = $(".postPartialContainer").attr('id');
var tag = $(".postPartialContainer").attr('alt');

document.getElementById("0").classList.remove("rounded");
document.getElementById("0").classList.add("rounded-bottom");

$(document).ready(function () {

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

        $(".likeBtn").off("click").on('click', function () {
            var $el = $(this);
            var slug = $el.data('slug');
            var postLikesCount = $(this).attr('postLikesCount');

            var e = $el.parents("#viewUpdate").children().children().children().children();
            var elId = e.attr('id');
            var pageCount = $("#" + elId).children(".spanPostPartial").children().html();
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

                    var e = $el.parents("#viewUpdate").children().children().children().children();
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

            var e = $el.parents("#viewUpdate").children().children().children().children(".usefulCount");
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

                    var e = $el.parents("#viewUpdate").children().children().children().children(".usefulCount");
                    var elId = e.attr('id');

                    $("#" + elId).children(".spanPostPartial").children(".postSpan").html(" " + postUsefulCount);


                },
                error: function (req, status, error) {
                    console.log(status)
                }
            });
        });

    })
