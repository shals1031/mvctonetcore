/// <reference path="jquery-2.1.1.min.js" />
$(window).load(function () {
    var selectedCons = [];
    // menu events
    $("#nav > li.has-sub > a").click(function (e) {
        if ($("body").hasClass("big")) {
            var self = $(this);
            $("#nav > li.open").not(self.parents("li")).find("> a").trigger("click");
            self.parent().toggleClass("open");
            self.parent().find("ul").toggle(200);
            self.find("i:nth-child(2)").toggleClass("fe-right-open fe-down-open");
        }
        e.preventDefault();
    });
    $(document).on("mouseenter", ".sml #nav > li.has-sub", function () {
        $(this).find("ul").show(200);
    });
    $(document).on("mouseleave", ".sml #nav > li.has-sub", function () {
        $(this).find("ul").stop(true, true).hide(200);
    });
    $(document).mouseup(function (e) {
        var container = $(".user-holder");
        if (!container.is(e.target) && container.has(e.target).length === 0) {
            if ($(".user-dropdown").is(".active"))
                $(".user-dropdown").trigger("click");
        }
    });
    $("#nav li a").each(function () {
        var self = $(this);
        if (self.attr("href") == window.location.pathname) {
            self.addClass("active");
            if (self.parents("li.has-sub").length > 0)
                self.parents("li.has-sub").addClass("active").find("> a").trigger("click");
        }
    });

    $('.menu-toggler a').tooltip({ placement: "right" })
    $(document).on("click", '.menu-toggler a, button.navicon-button', function (e) {
        $(this).toggleClass("open")
        var sidebar = $("#sidebar");
        var subHolder = $(".sub-holder");
        $("body").toggleClass("sml big");
        if (!$("body").hasClass("sml")) {
            sidebar.find("a.active").parents(".has-sub").find("> a").trigger("click");
            var windowWidth = window.innerWidth;
            if (windowWidth <= 768) {
                $("#nav").hide();
                sidebar[0].style.height = '60px';
            }
            else {                
                sidebar.animate({ width: 240 }, 200, function () {
                    sidebar.find("li > a > span, li > a > .nav-arrow").show();
                    $("#logo h1").show();
                });
                subHolder.animate({ left: 240 }, 200);
                $("#header").animate({ left: 240 }, 200);
                //.animate({ left: 250 }, 200);
                $(".listing-selected").animate({ left: 240 }, 200);
            }
        }
        else {
            $("#logo h1").hide();
            sidebar.find("li > a > span, li > a > .nav-arrow").hide()
            sidebar.find(".has-sub.open > a").trigger("click").addClass("active");
            var windowWidth = window.innerWidth;
            if (windowWidth <= 768) {
                $("#nav").show();
                sidebar[0].style.height = '100%';
            }
            else {
                $("#logo h1").hide();
                sidebar.find("li > a > span, li > a > .nav-arrow").hide()                
                sidebar.animate({ width: 75 }, 200);
                subHolder.animate({ left: 75 }, 200);
                $(".listing-selected, #header").animate({ left: 75 }, 200);
                //$(".overlay").addClass("big");//.animate({ left: 85 }, 200);
            }
        }        
        e.preventDefault();
    });

    //sub menu events and actions
    $(document).on("click", "#nav-settings", function (e) {
        e.preventDefault();
        var url = $(this).data("url");
        $.post(url, function (html) {
            $("body").append(html);
            $('.modal').modal({ show: true }).find("select").select2({ minimumResultsForSearch: -1, width: "resolve" });
            $.validator.unobtrusive.parse($("#settings-box form"));
        });
    });
    $('#subnav li a').tooltip({ placement: "bottom" });

    $(document).on('click', '.user-dropdown', function (e) {
        var self = $(this);
        self.toggleClass("active");
        self.parent().find("ul").toggle();
        e.preventDefault();
    });

    //modal stuff
    $(document).on('hidden.bs.modal', '.modal', function(e) {
        $(this).remove();
    });

    //datatables stuff
    $(document).on("change", "#status", function () {
        dTable.draw();
    });

    $(document).on("click", ".admin-tbl tbody td.more-details", function () {
        var self = $(this);
        var tr = self.parent();
        var row = dTable.row(tr);
        self.toggleClass("expanded");
        self.find("span i").toggleClass("fe-plus fe-minus");
        if (row.child.isShown()) {
            row.child.hide();
        }
        else {
            row.child(row.data().Additional, "sub-row animated fadeIn").show();
        }
    });

    //misc
    $(document).on('click', function (e) {
        if (!$(e.target).hasClass("dt-change") && !$(e.target).parents("a").hasClass("dt-change")) {
            $(".dt-change").popover('hide');
        }
        if (!$(e.target).hasClass("dt-print") && !$(e.target).parents("a").hasClass("dt-print")) {
            $(".dt-print").popover('hide');
        }
        if (!$(e.target).hasClass("dt-print-new") && !$(e.target).parents("a").hasClass("dt-print-new")) {
            $(".dt-print-new").popover('hide');
        }
        return;
    });
    $('#body').mutate('height', function (element, info) {
        $('.sub-holder').perfectScrollbar('update');
    });
    $('.sub-list-holder').mutate('height', function (element, info) {
        $('.sub-list-holder').perfectScrollbar('update');
    });
    $(window).resize(function () {
        $('.ps-container').perfectScrollbar('update');
    });
    $('.sub-holder').perfectScrollbar();

    $(document).on("click", ".tel-form .tel-tabs button:not(:disabled)", function (e) {
        var self = $(this);
        if (self.is(":not(.active)")) {
            var btns = $(".tel-form .tel-tabs button");
            btns.removeClass("active");
            self.addClass("active");
            $(".modal-footer .update").removeClass("team-btn link-btn");
            //$(".modal-footer .update").toggleClass("saver team-btn");
            var index = btns.index(self);
            $(".tel-form table.active").removeClass("active").hide(200, function () {
                $(".tel-form table").eq(index).addClass("active").show(200);
                if (self.is("#teams")) {
                    setupTeamActions(true);
                    $(".modal-footer .update").toggleClass("saver team-btn");
                }
            });
        }
        e.preventDefault();
    });
    $(document).on('change', '.tel-form #banksddl', function (e) {
        var bid = $(this).val();
        var url = $(".admin-tbl").data("url") + "/getbranchesforbank";
        $.get(url, { id: bid }, function (branches) {
            $("#Branch").select2("destroy");
            var html = "<option></option>";
            for (var x = 0; x < branches.length; x++) {
                html += "<option value='" + branches[x].Value + "'>" + branches[x].Text + "</option>";
            }
            $("#Branch").html(html).select2({ minimumResultsForSearch: -1, width: "resolve", placeholder: "Select an option" });
        });
    });
    $(document).on("click", ".tel-form .tel-date > button", function (e) {
        $('.tel-form .tel-date .date-box').datepicker("show");
        $('.tel-form .tel-date .date-box').datepicker().on('changeDate', function (ev) {
            $(this).datepicker('hide');
        });
        e.preventDefault();
    });
    $(document).on("click", ".tel-form .tel-img-uploader", function (e) {
        $(this).parent().find("input[type='file']").trigger("click");
        e.preventDefault();
    })
    $(document).on("change", ".tel-form input[type='file']", function () {
        if (this.files && this.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $('.tel-form .pic-box img').attr('src', e.target.result);
            }
            reader.readAsDataURL(this.files[0]);
        }
    });
});