jQuery.fn.tagName = function() {
    return this.prop("tagName");
};

// http://phiary.me/javascript-string-format/ より
// 存在チェック
if (String.prototype.format == undefined) {
    /**
     * フォーマット関数
     */
    String.prototype.format = function (arg) {
        // 置換ファンク
        var repFn = undefined;

        // オブジェクトの場合
        if (typeof arg == "object") {
            repFn = function (m, k) { return arg[k]; };
        }
        // 複数引数だった場合
        else {
            var args = arguments;
            repFn = function (m, k) { return args[parseInt(k)]; };
        }

        return this.replace(/\{(\w+)\}/g, repFn);
    };
}

$(document).ready(function () {
    if ($("dirs") !== null)
    {
        galleryLoad();
    }
});

function galleryLoad() {
    if (window.location.hash === "") {
        // Load only directory list on first visit.
        $.ajax("/API/{0}/Directory".format($("#lang").text())).done(function (data) {
            if (data.success) {
                for (var idx in data.directories) {
                    if (data.directories.hasOwnProperty(idx)) {
                        var dir = data.directories[idx];
                        var div = $('<div class="dir unknown"></div>').appendTo(".dirs");
                        $('<a href="#{0}">{1}</a>'.format(dir.path, dir.name)).appendTo(div).click(dirClick);
                    }
                }
            } else {
                $(".dirs").append("<p>failed to load directory</p>");
            }
        });
    } else {
        // Otherwise load last visited folder.
        var path = decodeURIComponent(window.location.hash).substr(1).split("|");
        var offset;
        if (path.length === 2) {
            offset = path[1];
        } else {
            offset = 0;
        }
        path = path[0];

        // Next page loader needs a fake <a> tag created and the image count div updated.
        $.ajax("/API/Count/{0}".format(path)).done(function (data) {
            if (data.success) {
                $("#count").html(data.count);
                // Can't just use the url hash as it might not have the |offset.
                var link = $('<a href="#{0}|{1}"></a>'.format(path, offset));
                loadDirectory(link);
            }
        });

        $.ajax("/API/{0}/Directory".format($("#lang").text())).done(function (data) {
            if (data.success) {
                for (var idx in data.directories) {
                    if (data.directories.hasOwnProperty(idx)) {
                        var dir = data.directories[idx];
                        var div = $('<div class="dir unknown"></div>').appendTo(".dirs");
                        // Add click handlers to load sub directories.
                        $('<a href="#{0}">{1}</a>'.format(dir.path, dir.name)).appendTo(div).click(dirClick);
                    }
                }

                // Start recursion.
                var startDir = path.split(":")[0];
                var target = $('a[href="#{0}"]'.format(startDir)).parent();
                loadSubDirs(target, path);
            } else {
                $(".dirs").append("<p>failed to load directory</p>");
            }
        });
    }
    

    // Act on all a clicks to handle the mess in the controls at the bottom.
    window.onclick = function (e) {
        if ($(e.target).hasClass("page")) {
            loadDirectory(e.target);
        }
    };
}

function dirClick() {
    var node = $(this).parent();
    loadDirectory(node);

    if ($(node).hasClass("unknown")) {
        loadSubDirs(node);
    } else if ($(node).hasClass("closed")) {
        open(node);
    } else if ($(node).hasClass("open")) {
        close(node);
    }
}

function loadSubDirs(node, fullPath) {
    $.ajax("/API/{0}/Directory/{1}".format($("#lang").text(), $("a", node).attr("href").substr(1))).done(function (data) {
        if (data.success) {
            if (data.directories.length > 0) {
                // Unloaded directory with sub directories.
                var content = $('<div class="content"></div>').appendTo(node);
                for (var idx in data.directories) {
                    if (data.directories.hasOwnProperty(idx)) {
                        var dir = data.directories[idx];
                        var div = $('<div class="dir unknown"></div>').appendTo(content);
                        $('<a href="#{0}">{1}</a>'.format(dir.path, dir.name)).appendTo(div).click(dirClick);
                    }
                }
                $(node).removeClass("unknown").addClass("open");

                // Optionally chain load to get to last directory.
                if (fullPath !== undefined) {
                    var parentLength = $("a", node).attr("href").split(":").length;
                    var fullSplit = fullPath.split(":");
                    if (fullSplit.length > parentLength) {
                        // Recurse.
                        var targetPath = "";
                        for (var i = 0; i < parentLength; i++) {
                            targetPath += "{0}:".format(fullSplit[i]);
                        }
                        targetPath += fullSplit[parentLength];

                        var target = $('a[href="#{0}"]'.format(targetPath)).parent();
                        console.log(targetPath, target);
                        loadSubDirs(target, targetPath);
                    }
                }
            } else {
                // Unloaded directory with no sub directories.
                $(node).removeClass("unknown").addClass("empty");
            }
        } else {
            $(node).append("<content>failed to load directory</content>");
        }
    });
}

function open(node) {
    $(node).removeClass("closed").addClass("open");
}

function close(node) {
    $(node).removeClass("open").addClass("closed");
}

function loadDirectory(node) {
    var pageSize = 50;

    // Don't reload on clicking the current directory.
    var path;
    if ( $(node).tagName() === "DIV" && !$(node).hasClass("active")) {
        path = $("a", node).attr("href").substr(1);
        $.ajax("/API/{0}/Images/{1}".format($("#strings #lang").text(), path)).done(function (data) {
            var images;
            var controls;
            if (data.success) {
                $(".active").removeClass("active");
                $(node).addClass("active");
                images = $(".images");
                images.empty();
                for (var idx in data.images) {
                    if (data.images.hasOwnProperty(idx)) {
                        var image = data.images[idx];
                        $(images).append(
                            '<a href="{0}"><img src="{1}"></a>'.format(image.path, image.thumb, image.name));
                    }
                }

                // Controls.
                controls = $(".controls");
                controls.empty();

                // As this is the first time the folder is loaded, we need the image count.
                $.ajax("/API/Count/{0}".format(path)).done(function (data) {
                    if (data.success) {
                        var pageCount = (((data.count - 1) / pageSize) >> 0) + 1;
                        // Previous and first page buttons, as this is loaded from the directory browser we know it's page 1.
                        var html = '<a class="page" href="#{0}|0">&lt;&lt;</a> <a class="page" href="#{0}|0">&lt;</a> '.format(path);
                        for (var i = 0; i < pageCount; i++) {
                            html += '<a class="page" href="#{0}|{1}">{2}</a> '.format(path, i * pageSize, i + 1);
                        }
                        html += '<a class="page" href="#{0}|{1}">&gt;</a> <a class="page" href="#{0}|{2}">&gt;&gt;</a> <br />'.format(path, pageSize, (pageCount - 1) * pageSize);
                        html += $("#strings #total").text().format(data.count);
                        controls.html(html);
                        $("#count").html(data.count);
                    } else {
                        controls.html("Couldn't load image count.");
                    }
                });
            } else {
                images = $(".images");
                images.empty();
                controls = $(".controls");
                controls.empty();
                controls.html("Couldn't load images.");
            }
        });
    } else if ($(node).tagName() === "A") {
        // One of the page markers was clicked.
        path = $(node).attr("href").substr(1).split("|");
        var offset = path[1];
        path = path[0];

        $.ajax("/API/{0}/Images/{1}/{2}".format($("#lang").text(), path, offset)).done(function (data) {
            var images;
            var controls;
            if (data.success) {
                images = $(".images");
                images.empty();
                for (var idx in data.images) {
                    if (data.images.hasOwnProperty(idx)) {
                        var image = data.images[idx];
                        $(images).append(
                            '<a href="{0}"><img src="{1}"></a>'.format(image.path, image.thumb, image.name));
                    }
                }

                // Controls.
                controls = $(".controls");
                controls.empty();

                // Reuse image count from last time.
                var count = $("#count").text();
                
                var pageCount = (((count - 1) / pageSize) >> 0) + 1;
                var prev = offset - pageSize;
                if (prev < 0) {
                    prev = 0;
                }
                var next = offset + pageSize;
                if (next > (count - pageSize)) {
                    next = count - pageSize;
                }
                var current = ((offset / pageSize) >> 0);
                // Previous and first page buttons, have to modify for page load.
                var html = '<a class="page" href="#{0}|0">&lt;&lt;</a> <a class="page" href="#{0}|{1}">&lt;</a> '.format(path, prev);
                for (var i = 0; i < pageCount; i++) {
                    if (i === current) {
                        html += '<a class="page current" href="#{0}|{1}">{2}</a> '.format(path, i * pageSize, i + 1);
                    } else {
                        html += '<a class="page" href="#{0}|{1}">{2}</a> '.format(path, i * pageSize, i + 1);
                    }
                    
                }
                html += '<a class="page" href="#{0}|{1}">&gt;</a> <a class="page" href="#{0}|{2}">&gt;&gt;</a> '.format(path, next, (pageCount - 1) * pageSize);
                html += "<br /><span>{0}</span> total".format(count);
                controls.html(html);
            } else {
                images = $(".images");
                images.empty();
                controls = $(".controls");
                controls.empty();
                controls.html("Couldn't load images.");
            }
        });
    }
}
