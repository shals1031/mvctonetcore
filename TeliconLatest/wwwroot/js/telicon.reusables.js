//extensions
Number.prototype.format = function (n, x) {
    var re = '\\d(?=(\\d{' + (x || 3) + '})+' + (n > 0 ? '\\.' : '$') + ')';
    return this.toFixed(Math.max(0, ~~n)).replace(new RegExp(re, 'g'), '$&,');
};
Number.prototype.formatMoney = function (places, symbol, thousand, decimal) {
    places = !isNaN(places = Math.abs(places)) ? places : 2;
    symbol = symbol !== undefined ? symbol : "$";
    thousand = thousand || ",";
    decimal = decimal || ".";
    var number = this,
	    negative = number < 0 ? "-" : "",
	    i = parseInt(number = Math.abs(+number || 0).toFixed(places), 10) + "",
	    j = (j = i.length) > 3 ? j % 3 : 0;
    return symbol + negative + (j ? i.substr(0, j) + thousand : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + thousand) + (places ? decimal + Math.abs(number - i).toFixed(places).slice(2) : "");
};
String.prototype.removeFormatting = function () {
    return parseFloat(this.replace(/[\$,]/g, ''));
}
String.prototype.isEmpty = function () {
    if (this == null || this.trim() == "" || this.trim().length < 1)
        return true;
    return false;
}
$.widget("ui.dialog", $.extend({}, $.ui.dialog.prototype, {
    _title: function (title) {
        if (!this.options.title) {
            title.html("&#160;");
        } else {
            title.html(this.options.title);
        }
    }
}));
$.validator.setDefaults({
    ignore: []
});

//global variables
var actsTempStorage = null;
var matsTempStorage = null;
var actsSelectedValue = [];
var temp;
var wochanged = false;
var dTable;
var aDtable;
var techTable;
var requiresNew = true;
var additional;
var additionalFunc;
var batch = false;
var deleteUrl = null;
var canEdit = 'true';
var isWO = false;
var isMatAddedToist = false;
var tempActs = [];
var tempLoc = "";
var tempLocs = [];
var tempDate = null;
var actMaterialQtyArry = [];
var dialogOptions = {
    dialogClass: "animated tada",
    width: 300,
    resizable: false,
    modal: true,
    close: function (event, ui) {
        $(this).remove();
    },
    buttons: [{ text: "OK", click: function () { $(this).dialog("close"); } }]
};
var dataTableOptions = {
    dom: '<"top"<"sub-top clearfix"fr>lp>t<"bottom clearfix"p>',
    pagingType: 'full_numbers',
    "autoWidth": false,
    "language": { "lengthMenu": "_MENU_" },
    "search": { "caseInsensitive": true },
    processing: true,
    paging: true,
    stateSave: true,
    serverSide: true,
    columnDefs: null,
    createdRow: null,
    order: null,
    langauge: {
        "processing": ""
    },
    ajax: {
        url: "", type: "POST",
        data: function (d) {
            d.additional = $("#status").val();
            d.year = $("#year").val();
            d.client = $("#clients").val();
            d.zone = $("#zone").val();
            d.clas = $("#clas").val();
            return $.toDictionary(d);
        }
    },
    drawCallback: function (settings) {
        $("[title]:not(.ui-dialog-titlebar [title])").tooltip({ placement: "top" });
    },
    preDrawCallback: function (e) {
        
    },
    footerCallback: null,
    initComplete: function (settings, json) {
        if (requiresNew)
            $(".dataTables_wrapper .sub-top").append("<button class='add-new'>New</button>");
        $(".dataTables_wrapper .top select").select2({ minimumResultsForSearch: -1, width: "resolve" });
        
    }
};

$(window).load(function () {
    
    //contractor statements
    $(document).on("click", ".manage-dets.tt-details, .manage-dets.tt-print", function (e) {
        loading(false);
        var self = $(this);
        var id = $(".tech-tbl").data("id");
        var pdate = self.parents("tr").attr("id");
        var url = "../technician/" + (self.hasClass("tt-details") ? "statementdetail" : "perioddetail");
        $.get(url, { id: id, pdate: pdate }, function (html) {
            removeLoader(false, 500, function () {
                $(".overlay-body").append(html);
                $(".overlay").addClass("everything").show(200, function () {
                    var olay = $(this);
                    setTimeout(function () {
                        olay.perfectScrollbar();
                    }, 200);
                });
            });
        });
        e.preventDefault();
    });
    $(document).on("click", ".manage-dets.tt-details, .manage-dets.tt-print-payslip", function (e) {
        //loading(false);
        var self = $(this);
        var id = $(".tech-tbl").data("id");
        var pdate = self.parents("tr").attr("id");
        var url = "../technician/GenerateTechPaySlipForPeriod";
        $.post(url, { id: id, pdate: pdate }, function (html) {
            removeLoader(false, 500, function () {
                //$(".overlay-body").append(html);
                //$(".overlay").addClass("everything").show(200, function () {
                //    var olay = $(this);
                //    setTimeout(function () {
                //        olay.perfectScrollbar();
                //    }, 200);
                //});
            });
        });
        e.preventDefault();
    });
    $(document).on("click", ".input-daterange .refresh", function () {
        techTable.draw(false);
    });
    $(document).on("click", ".input-daterange .cancel", function () {
        $(".input-daterange input").val("");
        $('.dataTables_wrapper .sub-top .input-daterange').datepicker('remove').datepicker({
            format: "DD M dd, yyyy"
        });
        techTable.fnDraw();
    });
    //print stuff
    $(document).on("click", ".print-holder .print-clicker", function (e) {
        window.print();
        e.preventDefault();
    });
    //user stuff 
    $(document).on("click", "#nav-password", function (e) {
        $.get("../auth/changepassword", function (html) {
            $("body").append(html);
            $('.modal').modal({ show: true });
            $.validator.unobtrusive.parse($(".tel-form form"));
        });
        e.preventDefault();
    });
    //print stuff
    $(document).on("click", ".hide-close", function () {
        var overlay = this.parentNode;
        if (overlay.id != undefined && overlay.id == "drillDown") {
            $("#drillDown").removeClass("everything report").hide(200).find(".overlay-body").html("");
            $(".overlay").show();
            if (batch == true) {
                var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); batchList() } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
                ];
                var msg = '<p>Do you want to <b class="default-col">Batch</b> the currently selected <b>Invoices</b>?</p></div>';
                displayConfirmationMsg(msg, 'Batching', buttons);
            }
        }
        else {
            $(".overlay").removeClass("everything report").hide(200).find(".overlay-body").html("");
            if (batch == true) {
                var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); batchList() } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
                ];
                var msg = '<p>Do you want to <b class="default-col">Batch</b> the currently selected <b>Invoices</b>?</p></div>';
                displayConfirmationMsg(msg, 'Batching', buttons);
            }
        }
    });
    $(document).on("click", ".print-holder .fk-checkbox", function (e) {
        var self = $(this);
        self.toggleClass("checked");
    });
    //datatable stuff
    $(document).on("click", ".dt-delete", function (e) {
        var self = $(this);
        var contractorName = $("#liContractorName").html() != undefined ? (" " + $("#liContractorName").html()) : '';
        var itemName = self.parents("tr").find(".dt-item-title").text() + contractorName;
        var itemType = self.parents("table").data("name");
        var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); deleteRow(self); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
        ];
        var msg = '<p>Are you sure you want to <b class="default-col">Delete</b> <b>' + itemName + '</b> record?</p></div>';
        displayConfirmationMsg(msg, 'Delete ' + itemType, buttons);
        e.preventDefault();
    });

    $(document).on("click", ".dt-delete-swo", function (e) {
        var self = $(this);
        var contractorName = $("#liContractorName").html() != undefined ? (" " + $("#liContractorName").html()) : '';
        var itemName = self.parents("tr").find(".dt-item-title").text() + contractorName;
        var itemType = self.parents("table").data("name");
        var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); deleteSWORow(self); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
        ];
        var msg = '<p>Are you sure you want to <b class="default-col">Delete</b> <b>' + itemName + '</b> record?</p></div>';
        displayConfirmationMsg(msg, 'Delete ' + itemType, buttons);
        e.preventDefault();
    });
    $(document).on("click", ".dt-edit", function (e) {
        var self = $(this);
        var row = self.parents("tr");
        var canContinue = true;
        if (isWO) {
            var elm = row[0].cells[3].children[0] != undefined ? row[0].cells[3].children[0].className : "";            
            if (elm == "fe-ok yes" && (canEdit == "False" || canEdit == "false"))
                canContinue = false;
        }
        if (canContinue) {
            loading(false);
            var url = $(".admin-tbl").data("url") + "/edit";
            var id = $(this).parents("tr").attr("id");
            id = id.indexOf("_") > -1 ? id.split("_")[1] : id;
            $.get(url, { id: id }, function (html) {
                removeLoader(false, 500, function () {
                    $("body").append(html);
                    $('.modal').modal({ show: true }).find("select").select2({ minimumResultsForSearch: -1, width: "resolve" });
                    $('.modal').find(".number").onlyNumbers({ neagtive: false, decimal: true });
                    $('.modal .date-box').datepicker({ format: "mm/dd/yyyy" }).on('changeDate', function (ev) {
                        $(this).datepicker('hide');
                    });                    
                    $.validator.unobtrusive.parse($(".tel-form form"));
                });
            });
        }
        else {
            var htmlMsg = "<p>This Workorder has already been <b>Submitted</b>.</p>";
            var htmlTitle = "<i class='fe-comment'></i>Already Submitted";
            displayDialogMsg(htmlMsg, htmlTitle);
        }
        e.preventDefault();
    });
    $(document).on("click", ".dt-edit-swo", function (e) {
        loading(false);
        //var url = $(".admin-tbl").data("url") + "/EditSWO";
        var id = $(this).parents("tr").attr("id");
        id = id.indexOf("_") > -1 ? id.split("_")[1] : id;
        window.open("/workorder/EditSWO?id=" + id, "_self");

        //$.get(url, { id: id }, function (html) {
        //    removeLoader(false, 500, function () {
        //        if (data.Additional > 0) {
        //            $(".tel-confirm-dialog").dialog("close");
        //            dTable.draw();
        //        }
        //        else {
        //            $(".tel-confirm-dialog").dialog("close");
        //            var htmlMsg = "<p>This record cannot be <b class='default-col'>Edited</b> because it is <b>not in</b> New status.</p>";
        //            var htmlTitle = "<i class='fe-comment'></i>Cannot Edit";
        //            displayDialogMsg(htmlMsg, htmlTitle);
        //        }
        ////        $("body").append(html);
        ////        $('.modal').modal({ show: true }).find("select").select2({ minimumResultsForSearch: -1, width: "resolve" });
        ////        $('.modal').find(".number").onlyNumbers({ neagtive: false, decimal: true });
        ////        $('.modal .date-box').datepicker({ format: "mm/dd/yyyy" });
        ////        $.validator.unobtrusive.parse($(".tel-form form"));
        //    });
        //});
        e.preventDefault();
    });
    $(document).on("click", ".dt-condeduction", function (e) {
        var url = "/contractordeduction/index?p_ConId=";
        var id = $(this).parents("tr").attr("id");
        id = id.indexOf("_") > -1 ? id.split("_")[1] : id;
        window.location.href = url + id;
    });
    $(document).on("click", ".dt-conpayslip", function (e) {
        var url = "/technician/techpayslip?p_ConId=";
        var id = $(this).parents("tr").attr("id");
        id = id.indexOf("_") > -1 ? id.split("_")[1] : id;
        window.location.href = url + id;
    });
    //form stuff
    $(document).on("click", ".dataTables_wrapper .sub-top .add-new, .dataTables_wrapper .sub-top .dropdown-menu>li>a", function (e) {
        var self = $(this);
        var url = self.is(".add-new") ? $(".admin-tbl, .tech-tbl").data("url") + "/create" : self.attr("href");
        url += typeof self.data("type") === 'undefined' ? "" : "?type=" + self.data("type");
        loading(true);
        $.get(url, function (html) {
            removeLoader(true, 200, function () {
                $("body").append(html);
                $('.modal').modal({ show: true }).find("select").select2({ minimumResultsForSearch: -1, width: "resolve", placeholder: "Select an option" });
                $('.modal').find(".number").onlyNumbers({ neagtive: false, decimal: true });
                $('.modal .date-box').datepicker({ format: "mm/dd/yyyy" }).on('changeDate', function (ev) {
                    $(this).datepicker('hide');
                });
                
                $.validator.unobtrusive.parse($(".tel-form form"));
            });
            setTimeout('$($(".long")[0]).focus()', 1500);
        });
        e.preventDefault();
    });
    $(document).on("click", ".dataTables_wrapper .sub-top .add-new-swo", function (e) {
        loading(false);
        window.open("/workorder/createNewSWO", "_self");
        e.preventDefault();
    });
    $(document).on("submit", ".tel-form form", function (e) {
        var self = $(this);
        if (isWO) {
            SubmitWO(self, false);
            e.preventDefault();
            return;
        }
        self.validate();
        var data = new FormData(self.get(0));
        if (self.valid()) {
            $(".tel-form button.saver, .tel-form input, tel-form select").attr("disabled", "disabled");
            $.ajax({
                url: self.attr("action"),
                data: data,
                type: "post",
                cache: false,
                contentType: false,
                processData: false,
                success: function (data) {
                    $(".tel-form button.saver, .tel-form input, tel-form select").removeAttr("disabled");
                    if (data.Code != "100") {
                        if (data.Code == "999")
                            setupMsgBar(3, data.Additional);
                        else if(data.Code == '1436')
                            setupMsgBar(3, "Create/Update failed. You don't have permission to perform this action.");
                        else
                            setupMsgBar(3, "Create/Update failed. Please try again or contact your system administrator");
                    }
                    else {
                        setupMsgBar(2, "Create/Update was completed successfully!");
                        if (data.Additional == "" || data.Additional.length < 0 || data.Additional == null) {
                            $(".tel-form input").val("");
                            var page = dTable.page();
                            dTable.page(page).draw(false);
                            //dTable.draw();
                            setTimeout('$(".close").click()', 2000);
                        }
                        else if (data.Additional.toString().substr(0, 1) == "#")
                            setupMsgBar(3, data.Additional.substr(1));
                        else if (typeof data.Additional === 'string' || !isNaN(data.Additional)) {
                            $(".tel-form .tel-form-id").val(data.Additional);
                            if (!isWO) {
                                var page = dTable.page();
                                dTable.page(page).draw(false);
                                //dTable.draw();
                                setTimeout('$(".close").click()', 2000);
                            }
                            else {
                                var self = $("#teams");
                                var btns = $(".tel-form .tel-tabs button");
                                btns.removeClass("active");
                                self.addClass("active");
                                $(".modal-footer .update").removeClass("team-btn link-btn");
                                $(".tel-form table.active").removeClass("active").hide(200, function () {
                                    $(".tel-form table").eq(1).addClass("active").show(200);
                                    if (self.is("#teams")) {
                                        setupTeamActions(true);
                                        $(".modal-footer .update").toggleClass("saver team-btn");
                                    }
                                });
                            }
                            isWO = false;
                        }
                        else {
                            for (var prop in data.Additional) {
                                $(".tel-form [name='" + prop + "']").val(data.Additional[prop]);
                            }
                            var page = dTable.page();
                            dTable.page(page).draw(false);
                            //dTable.draw();
                            setTimeout('$(".close").click()', 2000);
                        }
                        $(".tel-form .tel-tabs button").removeAttr("disabled");
                    }
                }
            });
        }
        e.preventDefault();
    });
    function SubmitWO(self, fromTeam) {
        self = $("#wOForm");
        self.validate();
        var data = new FormData(self.get(0));
        if (self.valid()) {
            $(".tel-form button.saver, .tel-form input, tel-form select").attr("disabled", "disabled");
            $.ajax({
                url: self.attr("action"),
                data: data,
                type: "post",
                cache: false,
                contentType: false,
                processData: false,
                success: function (data) {
                    $(".tel-form button.saver, .tel-form input, tel-form select").removeAttr("disabled");
                    if (data.Code != "100") {
                        if (data.Code == "999")
                            setupMsgBar(3, data.Additional);
                        else
                            setupMsgBar(3, "Create/Update failed. Please try again or contact your system administrator");
                    }
                    else {
                        setupMsgBar(2, "Create/Update was completed successfully!");
                        if (data.Additional == "" || data.Additional.length < 0 || data.Additional == null)
                            $(".tel-form input").val("");
                        else if (data.Additional.toString().substr(0, 1) == "#")
                            setupMsgBar(3, data.Additional.substr(1));
                        else if (typeof data.Additional === 'string' || !isNaN(data.Additional)) {
                            $(".tel-form .tel-form-id").val(data.Additional);
                            if (!fromTeam) {
                                var self = $("#teams");
                                var btns = $(".tel-form .tel-tabs button");
                                btns.removeClass("active");
                                self.addClass("active");
                                $(".modal-footer .update").removeClass("team-btn link-btn");
                                $(".tel-form table.active").removeClass("active").hide(200, function () {
                                    $(".tel-form table").eq(1).addClass("active").show(200);
                                    if (self.is("#teams")) {
                                        setupTeamActions(true);
                                        $(".modal-footer .update").toggleClass("saver team-btn");
                                    }
                                });
                            }
                            else {
                                setTimeout('$(".close").click()', 2000);
                            }
                            isWO = false;
                        }
                        else {
                            for (var prop in data.Additional) {
                                $(".tel-form [name='" + prop + "']").val(data.Additional[prop]);
                            }
                            var page = dTable.page();
                            dTable.page(page).draw(false);
                            setTimeout('$(".close").click()', 2000);
                        }
                        $(".tel-form .tel-tabs button").removeAttr("disabled");
                    }
                }
            });
        }
        //e.preventDefault();
    }
    $(document).on("click", ".tel-form button.saver", function (e) {
        //if (isWO) {
        //    //SaveWO();
        //}
        //else {
            var form = $(".tel-form form");
            form.submit();
            setValidationMsgsForForm(form);
        //}
    });
    $(document).on("keyup, change", ".tel-form form td > input, .tel-form form div > input", function (e) {
        var self = $(this);
        if (!self.valid())
            setValidationMsgsForElm(self);
        else
            self.tooltip("destroy");
    });
    $(document).on("click", ".tel-checkbox", function (e) {

        var self = $(this);
        self.toggleClass("checked");
        self.find(":hidden").val(self.is(".checked") ? true : false);
        e.preventDefault();
    });
    $(document).on("click", ".tel-radio", function (e) {
        var self = $(this);
        var name = self.find(":hidden").attr("name");
        $(".tel-radio").not(self).removeClass("checked").find(":hidden").val(false);
        self.toggleClass("checked");
        self.find(":hidden").val(self.is(".checked") ? true : false);
        e.preventDefault();
    });

    //the hard stuff
    //constructor
    $(document).on("click", ".const-main .const-holder ul li > .wo-actions .add-materials button", function (e) {
        console.log("asfasf")
        var self = $(this);
        self.toggleClass("off");
        var parent = self.parents("li").find(".wo-actions");
        var matActn = parent.find(".mat-action");
        if (self[0].classList.length == 0) {
            LoadMetarialUsagePopup(self);            
            if (matActn.length < 1) {
                parent.append('<a href="#" class="mat-action"><i class="mdi mdi-server" "></i></a>');//style="color: green;
            }
            else
                $(matActn[0]).show();
        }
        else {
            if (!(matActn.length < 1)) {
                $(matActn[0]).hide();
            }
        }
        e.preventDefault();
    });
    $(document).on("click", "#constructor .title-editor button", function (e) {
        var self = $(this);
        self.parent().find("input").animate({ width: self.hasClass("active") ? 0 : 300 }, 200, "easeInOutBack", function () {
            self.toggleClass("active");
            $(this).toggleClass("active");
        });
        e.preventDefault();
    });
    $(document).on("click", ".has-constructor .dt-details, #con-wo .dt-details", function (e) {
        var self = $(this);
        var wid = self.parents("tr").attr("id");
        wid = wid.toLowerCase().indexOf("invoiced") > -1 ? "invoiced" : wid.split("_")[1];
        if (wid != "invoiced") {
            loading(false);
            url = self.parents("table").data("wurl");
            $("body").append("<div class='full-overlay'></div>");
            $(".full-overlay").fadeIn(200);
            $.ajax({
                url: url,
                data: { wid: wid },
                type: "POST",
                success: function (html) {
                    removeLoader(false, 500, function () {
                        $("body").append(html);
                        $("#constructor [title]").tooltip({ placement: "top" });
                        $("#constructor").addClass("animated bounceInRight").show();
                        $('.const-main .const-holder ul').perfectScrollbar();
                        $("#constructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                        $("#constructor .title span").dotdotdot();
                    });
                }
            });
        }
        else {
            var htmlMsg = "<p>This Workorder has already been <b>Invoiced</b>. To edit the Workorder you must first reverse the Invoice.</p>";
            var htmlTitle = "<i class='fe-comment'></i>Already Processed";
            displayDialogMsg(htmlMsg, htmlTitle);
        }
        e.preventDefault();
    });

    //constrcutor items
    $(document).on("click", "#constructor .const-holder.const-dates ul li:not(.active)", function (e) {
        var stop = discardDataCheck("acts");
        temp = $(this);
        if (tempDate != null) {
            var activeDate = tempDate;            
            $(".const-main .const-holder.const-locs ul li").each(function () {
                var self = $(this);
                var activeLoc = $(self.find("span")[0]).text();
                if (!CheckLocInTheArry(activeLoc, activeDate)) {
                    tempLocs.push({
                        location: activeLoc,
                        actDate: activeDate,
                        comment: $(self.find("span")[1]).text(),
                    });
                }
            });
        }

        //if (stop) {
        //    var buttons = [
        //        { text: "Continue", click: function () { $(this).dialog("close"); generateLocations(); } },
        //        { text: "Cancel", click: function () { $(this).dialog("close"); } }
        //    ];
        //    var msg = '<p>Are you sure you want to continue this action. Any unsaved changes will be <b>Discarded</b>!</p></div>';
        //    displayConfirmationMsg(msg, 'Discard Changes', buttons);
        //}
        //else {
            generateLocations();
        //}
            tempDate = temp.data("date");
    });
    
    $(document).on("click", "#constructor .const-holder.const-locs ul li", function (e) {
        
        var stop = discardDataCheck("locs");
        temp = $(this);
        //if (stop) {
            //var buttons = [
            //    { text: "Continue", click: function () { $(this).dialog("close"); generateActivities(); } },
            //    { text: "Cancel", click: function () { $(this).dialog("close"); } }
            //];
            //var msg = '<p>Are you sure you want to continue this action. Any unsaved changes will be <b>Discarded</b>!</p></div>';
            //displayConfirmationMsg(msg, 'Discard Changes', buttons);

            //$("#constructor .const-locs ul li.new-loc, #constructor .const-acts ul li.new-act").length > 0
            
            if (tempLoc != "") {
                var activeDate = $("#constructor .const-holder.const-dates ul li.active").data("date");
                var activeLoc = tempLoc;
                $(".const-main .const-holder.const-acts ul li").each(function () {
                    var self = $(this);
                    var iD = self.data("id");
                    var actID = self.find("select").val();
                    var actQty = self.find(".act-amt").val();
                    var matsReq = self.find(".mats-req") != undefined ? self.find(".mats-req").val() : false;
                    if (!actID.isEmpty() && !actQty.isEmpty() && !CheckActInTheArry(actID, activeLoc, activeDate)) {
                        tempActs.push({
                            id: iD,
                            location: activeLoc,
                            actDate: activeDate,
                            actID: actID,
                            actQty: actQty,
                            matsReq: matsReq
                        });
                    }
                });
            }
            generateActivities();
        //}
        //else {
        //    generateActivities();
        //}
        //var actLoc = $($(".const-main .const-holder.const-locs ul li.active").find("span")[0]).text();
        tempLoc = $(temp.find("span")[0]).text();
    });
    function CheckActInTheArry(actId, loc, date) {
        var inArry = false;
        for (var i = 0; i < tempActs.length; i++) {
            if (tempActs[i].actID == actId && tempActs[i].location == loc && tempActs[i].actDate == date) {
                inArry = true;
                break;
            }
        }
        return inArry;
    }
    function CheckLocInTheArry(loc, date) {
        var inArry = false;
        for (var i = 0; i < tempLocs.length; i++) {
            if (tempLocs[i].location == loc && tempLocs[i].actDate == date) {
                inArry = true;
                break;
            }
        }
        return inArry;
    }
    //constructor item actions
    $(document).on("click", "#constructor .const-dates > .title > a, #constructor .const-dates .placeholder button", function (e) {
        var self = $(this);
        setupActivityCalendar(null);
        e.preventDefault();
    });
    $(document).on("click", "#constructor .const-locs > .title > a, #constructor .const-locs .placeholder button", function (e) {
        
        if ($("#constructor .const-dates ul li.active").length == 0) {
            var msg = "<p>Please add and/or select a <b>Date</b> before attempting to add any new <b>Locations</b>.</p>";
            var title = "<i class='fe-comment'></i> Cannot Add Location";
            displayDialogMsg(msg, title);
        }
        else {
            var self = $(this);
            setupLocationDialogBox("", "");
        }
        e.preventDefault();
    });
    $(document).on("click", "#constructor .const-acts > .title > a, #constructor .const-acts .placeholder button", function (e) {
        
        if ($("#constructor .const-locs ul li.active").length == 0) {
            var msg = "<p>Please add and/or select a <b>Location</b> before attempting to add any new <b>Activities</b>.</p>";
            var title = "<i class='fe-comment'></i> Cannot Add Activity";
            displayDialogMsg(msg, title);
        }
        else {
            var actsHolder = $("#constructor > .const-main .const-acts");
            var self = $(this);
            if (actsHolder.find("ul").length == 0)
                actsHolder.append("<ul></ul>");
            if (actsTempStorage == null) {
                $.ajax({
                    async: false,
                    url: '/workorder/getactivities',
                    data: { wid: $("#constructor").data("id") },
                    type: 'post',
                    success: function (r) {
                        actsTempStorage = r;
                    }
                })
            }
            var ph = $("#constructor .const-acts .placeholder");
            var time = ph.is(":visible") ? 300 : 10;
            ph.hide(200);
            setTimeout(function () {
                actsHolder.find("ul").append(applyActivityTemplate(actsTempStorage));
                $('.const-main .const-acts ul').perfectScrollbar("update");
                var n = $('.const-main .const-acts ul').height();
                $('.const-main .const-acts ul').animate({ scrollTop: n }, 50);

                setDisableActivityOptions();
                actsHolder.find("select").select2({ width: "resolve", placeholder: "Select an Activity" });
                actsHolder.find("input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                //actsHolder.find("input.act-amt").on("blur", function (e) {
                //    var self = $(this);
                //    var prnt = $(this.parentElement);
                //    var id = prnt.find("select").val();
                //    $.post("/workorder/GetMaxQty", { actId: id }, function (r) {
                //        var msg = "";
                //        var title = "<i class='fe-comment'></i>Quntity Warning!"
                //        if (r.MaxQty != 0) {
                //            if (Number(self.val()) > r.MaxQty)
                //                msg = "Quntity cannot be greater than " + r.MaxQty + ".";
                //        }
                //        if (Number(self.val()) < 1)
                //            msg = "Quntity cannot be 0 or minus value.";
                //        if (msg != "") {
                //            displayDialogMsg(msg, title);
                //            self.val(1.00);
                //        }
                //    });
                //});
            }, time);
        }
        e.preventDefault();
    });
    $(document).on("change", "#constructor .const-acts select", function () {
        var self = $(this);
        var id = self.val();
        temp = self.parents("li");
        setDisableActivityOptions();
        $.get("/workorder/hasmaterial", { id: id }, function (result) {
            var parent = self.parents("li").find(".wo-actions");
            if (result.hasMats == true) {
                if (parent.find(".mat-action").length < 1) {
                    var str = '<div class="add-materials-contn">'
                                + '<div class="add-materials">'
                                + '<label>'
                                //+ '<span>Materials</span>'
                                //+ '<button>'
                                //+ '<span></span>'
                                //+ '</button>'
                                //+ '<input type="hidden" value="" />'
                                + '</label>'
                                + '</div>'
                                + '</div>';
                    parent.append(str + '<input type="hidden" class="mats-req" value="' + result.matsReq + '"/><a href="#" class="mat-action" title="Materials"><i class="mdi mdi-server"></i></a>');// style="color: green;"
                    //LoadMaterialPopup(temp);
                }
            }
            else {
                parent.find(".mat-action").remove();
            }
        });
    });

    //constructor top btns
    $(document).on("click", "#constructor > .title .btn-group label:not(.active)", function (e) {
        var self = $(this);
        var selfCls = self.attr("class");
        var type = selfCls.indexOf("unsubmit") > -1 ? "unsubmit" : selfCls.indexOf("verify") > -1 ? "verify" : "submit";
        var canContinue = true;
        if (type == "verify") {
            canContinue = false;
            CheckMaxQtyExceeded();
        }
        if (canContinue) {
            ChangeWOStatus(type);
        }
    });
    function CheckMaxQtyExceeded() {
        var id = $("#constructor").data("id");
        $.post("/workorder/CheckMaxQtyExceeded", { id: id }, function (r) {
            if (r.msg != "") {
                var buttons = [
                        { text: "Continue", click: function () { CheckAuthentication(this) } },
                        {
                            text: "Cancel", click: function () {
                                $(this).dialog("close");
                                $("#constructor > .title .btn-group label").removeClass("active");
                                $("#constructor > .title .btn-group label.submit").addClass("active");
                            }
                        }
                ];
                var msg = "<div><p>Cannot change the status, some <b class='default-col'>Activities</b> or <b class='default-col'>Materials</b> maximum quantity <b class='default-col'>Exceeded</b>.</p>" +
                    "<br />" +
                    "<div class='txt-holder'>"+
                    "<input type='text' placeholder='Username' style='height:30px;' id='txtUsername' />"+
                    "<i class='fe-user'></i>"+
                    "</div>" +
                    //"<br />" +
                    "<div class='txt-holder'>"+
                    "<input type='password' placeholder='Password' style='height:30px; margin-top:5px;' id='txtPassword' />" +
                    "<i class='fe-lock-open'></i>"+
                    "</div>"+
                    "</div>";
                displayConfirmationMsg(msg, 'Authentication Required', buttons);                
            }
            else
                ChangeWOStatus("verify");
        });
    }
    function ChangeWOStatus(type) {
        $.post("/workorder/changeStatus", { type: type, wid: $("#constructor").data("id") }, function (r) {
            if (r.Msg != "") {
                $("#constructor > .title .btn-group label").removeClass("active");
                if (type == "submit")
                    $("#constructor > .title .btn-group label.submit").addClass("active");
                else if (type == "verify")
                    $("#constructor > .title .btn-group label.verify").addClass("active");
                else
                    $("#constructor > .title .btn-group label.unsubmit").addClass("active");
                var msg = r.Msg;
                var title = "<i class='fe-comment'></i> Cannot Change Status!"
                if (r.Code == "1001")
                    title = "<i class='fe-comment'></i>Penalty Warning!"
                displayDialogMsg(msg, title);
            }
        });
    }
    function CheckAuthentication(e) {
        //$(this).dialog("close");
        var self = $(e);
        var uName = self.find("#txtUsername").val();
        var pWord = self.find("#txtPassword").val();
        if (uName == "" || pWord == "")
            return;
        $.post("/workorder/CheckAuthentication", { uName: uName, pWord:pWord }, function (r) {
            if (r.msg != "") {
                self.dialog("close");
                var buttons = [
                        { text: "Continue", click: function () { CheckAuthentication(this) } },
                        {
                            text: "Cancel", click: function () {
                                $(this).dialog("close");
                                $("#constructor > .title .btn-group label").removeClass("active");
                                $("#constructor > .title .btn-group label.submit").addClass("active");
                            }
                        }
                ];
                var msg = "<div><p>Cannot change the status, some <b class='default-col'>Activities</b> or <b class='default-col'>Materials</b> maximum quantity <b class='default-col'>Exceeded</b>.</p>" +
                    "<br />" +
                    "<div class='txt-holder'>" +
                    "<input type='text' placeholder='Username' style='height:30px;' id='txtUsername' />" +
                    "<i class='fe-user'></i>" +
                    "</div>" +
                    //"<br />" +
                    "<div class='txt-holder'>" +
                    "<input type='password' placeholder='Password' style='height:30px; margin-top:5px;' id='txtPassword' />" +
                    "<i class='fe-lock-open'></i>" +
                    "</div>" +
                    "</div>";
                displayConfirmationMsg(msg, 'Authentication Required', buttons);
            }
            else {
                self.dialog("close");
                ChangeWOStatus("verify");
            }
        });
    }
    //constructor edit and delete actions
    $(document).on("click", ".const-main .const-holder ul li .edit-action", function (e) {
        var self = $(this);
        temp = self.parents("li");
        var type = self.parents(".const-holder").hasClass("const-dates") ? "Date" : "Location";
        switch (type) {
            case "Date":
                setupActivityCalendar(self.parents("li").data("date"));
                break;
            default:
                setupLocationDialogBox($(self.parents("li").find("span")[0]).text(), $(self.parents("li").find("span")[1]).text());
                break;
        }
        e.stopPropagation();
        e.preventDefault();
    });
    $(document).on("click", ".const-main .const-holder ul li .del-action", function (e) {
        var self = $(this);
        temp = self.parents("li");
        if (temp.attr("class").indexOf("new") > -1) {
            temp.remove();
        }
        else {
            var holder = self.parents(".const-holder");
            var type = holder.hasClass("const-dates") ? "Date" : holder.hasClass("const-locs") ? "Location" : "Activity";
            var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); deleteAction(type); } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msgAddl = "";
            if (type == "Date")
                msgAddl = "This action will also <b class='default-col'>Delete</b> all <b class='loc-col'>Locations</b>, <b class='act-col'>Activities</b> and <b class='act-col'>Materials</b> related to this <b class='date-col'>Date</b>.";
            else if (type == "Location")
                msgAddl = "This action will also <b class='default-col'>Delete</b> all <b class='act-col'>Activities</b> and <b class='act-col'>Materials</b> related to this <b class='loc-col'>Location</b>.";
            else
                msgAddl = "This action will also <b class='default-col'>Delete</b> all <b class='act-col'>Materials</b> related to this <b class='act-col'>Activity</b>."
            var msg = "<div><p>" + msgAddl + " Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Delete ' + type, buttons);
        }
        e.stopPropagation();
        e.preventDefault();
    });
    $(document).on("click", ".const-main .const-holder ul li .mat-action", function (e) {
        var self = $(this);
        temp = self.parents("li");
        LoadMaterialPopup(temp);
        e.preventDefault();
    });
    function LoadMaterialPopup(temp) {
        var date = $("#constructor .const-holder.const-dates ul li.active").data("date");
        var loc = $($(".const-main .const-holder.const-locs ul li.active").find("span")[0]).text();
        var actId = temp.find("select").val();
        var wActId = $(temp).data("id");
        var id = $("#constructor").data("id");
        var invID = $("#constructor .wo-inv select").val();
        var type = $("#constructor").hasClass("view-wo") ? "view" : "";
        var qtyt = temp.find(".act-amt").val();
        var qty = qtyt != undefined && qtyt != "" ? Number(qtyt) : qtyt;
        loading(false);
        $("body").append("<div class='full-overlay'></div>");
        $(".full-overlay").fadeIn(200);
        $.ajax({
            url: "/WorkOrder/GenerateMetarialConstructor",
            data: { wActid: wActId, date: date, loc: loc, actId: actId, invID: invID, type: type, wid: id, qty: qty },
            type: "POST",
            success: function (html) {
                removeLoader(false, 500, function () {
                    isMatAddedToist = false;
                    $("body").append(html);
                    $("#matConstructor [title]").tooltip({ placement: "top" });
                    $("#matConstructor").addClass("animated bounceInRight").show();
                    $('.mat-const-main .const-holder ul').perfectScrollbar();
                    $("#matConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                    $("#matConstructor .title span").dotdotdot();
                    var matsHolder = $("#matConstructor > .mat-const-main .const-holder.const-mats");
                    var ph = matsHolder.find(".placeholder");
                    var time = ph.is(":visible") ? 300 : 10;
                    ph.find("button").removeClass("activated animated flash");
                    ph.hide(200);
                    var hasExisting = false;
                    if (actMaterialQtyArry.length > 0) {
                        if (matsHolder.find("ul").length == 0)
                            matsHolder.append("<ul></ul>");
                        var actID = $("#matConstructor #actIdHidden").val();
                        var loc = $("#matConstructor #locHidden").val();
                        var date = $("#matConstructor #dateHidden").val();
                        for (var i = 0; i < actMaterialQtyArry.length; i++) {
                            if (actMaterialQtyArry[i].loc == loc && actMaterialQtyArry[i].date == date && actMaterialQtyArry[i].actID == actID) {
                                if (!CheckIsMatInPlaceHolder(actMaterialQtyArry[i].matID)) {
                                    matsHolder.find("ul").append(applyMaterialTemplateWithValues(matsTempStorage, actMaterialQtyArry[i].matID, actMaterialQtyArry[i].matQty));
                                    $('.mat-const-main .const-mats ul').perfectScrollbar();
                                    setDisableMetarialOptions();
                                    matsHolder.find("select").select2({ width: "resolve", placeholder: "Select an Activity" });
                                    hasExisting = true;
                                }
                                else {
                                    $(".mat-const-main .const-holder.const-mats ul li").each(function () {
                                        var self = $(this);
                                        var matID = self.find("select").val();
                                        if (actMaterialQtyArry[i].matID == matID) {
                                            self.find(".act-amt").val(actMaterialQtyArry[i].matQty)
                                        }
                                    });
                                }
                            }
                        }
                    }
                    if (matsHolder.find("li").length == 0 && !hasExisting) {
                        matsHolder.find("ul").remove();
                        $(".mat-const-main .const-mats .placeholder").show(100, function () {
                            $(".mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
                        });
                    }
                    else {
                        setTimeout(function () {
                            setDisableMetarialOptions();
                            matsHolder.find("select").select2({ width: "resolve" });
                            matsHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                            $('.mat-const-main .const-mats ul').perfectScrollbar();
                        }, time);
                    }
                });
            },
            error: function (e) {
                var aasdasd = e;
            }
        });
    }
    function CheckIsMatInPlaceHolder(matId) {
        var inPlace = false;
        $(".mat-const-main .const-holder.const-mats ul li").each(function () {
            var self = $(this);
            var matID = self.find("select").val();
            if (matId == matID) {
                inPlace = true;
            }
        });
        return inPlace;
    }
    function applyMaterialTemplateWithValues(mats, matId, matQty) {

        var matBox = '<li class="clearfix new-act"><div class="act-holder"><select class="act-ddl" name="acts"><option></option>';
        for (x = 0; x < mats.length; x++) {
            var mat = mats[x];
            matBox += '<option value="' + mat.activityID + '" ' + (mat.activityID == matId ? 'selected="selected"' : '') + '>' + mat.description + '</option>';
        }
        matBox += '</select><input type="text" class="act-amt" value="' + matQty + '" /></div>' +
            '<div class="wo-actions"><a href="#" class="del-action"><i class="fe-trash"></i></a></div></li>';
        return matBox;
    }

    $(document).on("blur", "#constructor .const-acts input.act-amt", function (e) {
        var self = $(this);
        temp = self.parents("li");
        var prnt = $(this.parentElement);
        var id = prnt.find("select").val();
        $.post("/workorder/GetMaxQty", { actId: id }, function (r) {
            var msgAddl = "";
            if (r.MaxQty != 0) {
                if (Number(self.val()) > r.MaxQty)
                    msgAddl = "You have exceed the <b class='default-col'>Maximum Quantity ( " + r.MaxQty + " )</b> level for the activity.";
            }
            if (Number(self.val()) < 1)
                msgAddl = "You are going to insert <b class='default-col'>Nagavite</b> value as activity quantity.";
            if (msgAddl != "") {
                var buttons = [
                        { text: "Continue", click: function () { $(this).dialog("close"); ContinueFun(r.hasMats) } },
                        { text: "Cancel", click: function () { $(this).dialog("close"); self.val(r.MaxQty); } }
                ];
                var msg = "<div><p>" + msgAddl + " Are you sure you want to Continue?</p></div>";
                displayConfirmationMsg(msg, 'Quantity Warning', buttons);
            }
            else
                if (r.hasMats == true)
                    LoadMaterialPopup(temp);
        });
    });
    function ContinueFun(hasMats) {
        if (hasMats == true)
            LoadMaterialPopup(temp);
    }

    //constructor bottom btns
    $(document).on("click", "#constructor .process", function (e) {
        var id = $("#constructor").data("id");
        loading(true);
        $.get("../workorder/workorderactivitycount", { id: id }, function (data) {
            if (!data.any || !data.verified) {
                var title = "<i class='fe-comment'></i> Cannot Process Work Order!"
                var msg = "";
                removeLoader(true, 500, function () {
                    if (!data.any)
                        msg = "<p>Cannot <b class='default-col'>Process</b> a Work Order that has no activities. Please make sure that at least one (1) <b class='act-col'>Activity</b> was created.</p>";
                    else
                        msg = "<p>Cannot <b class='default-col'>Process</b> a Work Order that has not yet been <b class='act-col'>Verified</b>. Please make sure that you have changed the Work Order's status.</p>";
                    displayDialogMsg(msg, title);
                });
            }
            else {
                var invID = $("#constructor .wo-inv select").val();
                var title = $("#constructor .title-editor input").val();
                //var buttons = [
                //            {
                //                text: "Continue", click: function () {
                //                    $(this).dialog("close");
                                    loading(true);
                                    $.post("../workorder/processactivities", { id: id, invID: invID, title: title }, function (data) {
                                        removeLoader(true, 500, function () {
                                            if (typeof data.Additional === 'undefined') {
                                                closeConstructor();
                                                //$("#constructor").remove();
                                                //$("body").append(data);
                                                //$("#constructor").show();
                                            }
                                            msg = "<p>Successfully <b class='default-col'>Processed</b> Workorder. </p>";
                                            var title = "<i class='fe-comment'></i> Success!"
                                            displayDialogMsg(msg, title);
                                        });
                                    });
                //                }
                //            },
                //            { text: "Cancel", click: function () { $(this).dialog("close"); } }
                //];
                //var msg = '<p>Are you sure you want to <b class="default-col">Process</b> this Work Order?</p></div>';
                //removeLoader(true, 500, function () {
                //    displayConfirmationMsg(msg, 'Process Work Order', buttons);
                //});
            }
        });
        e.preventDefault();
    });
    $(document).on("click", "#constructor .cancel", function (e) {
        var stop = discardDataCheck("full");
        if (stop) {
            var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); closeConstructor(); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msg = '<p>Are you sure you want to continue this action. Any unsaved changes will be <b>Discarded</b>!</p></div>';
            displayConfirmationMsg(msg, 'Discard Changes', buttons);
        }
        else {
            closeConstructor();
        }
        e.preventDefault();
    });
    
    $(document).on("click", "#constructor .update", function (e) {
        
        var activeDate = $(".const-main .const-holder.const-dates ul li.active");
        var activeLoc = $(".const-main .const-holder.const-locs ul li.active");
        var acts = [];
        $(".const-main .const-holder.const-acts ul li").each(function () {
            var self = $(this);
            var actID = self.find("select").val();
            var actQty = self.find(".act-amt").val();
            if (!actID.isEmpty() && !actQty.isEmpty()) {
                acts.push({
                    actID: actID,
                    actQty: actQty
                });
            }
        });
        if (activeDate.length > 0 && activeLoc.length > 0 && acts.length > 0) {
            var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); updateAction(); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msg = "<p>Only the selected <b class='date-col'>Date</b>, <b class='loc-col'>Location</b> and valid <b class='act-col'>Activities</b> will be <b class='default-col'>Updated</b>. Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Update Confirmation', buttons);
        }
        else {
            var msg = "<p>Cannot <b class='default-col'>Update</b> incomplete Workorder. Please make sure that a valid <b class='date-col'>Date</b> and <b class='loc-col'>Location</b> were select and that at least one (1) <b class='act-col'>Activity</b> was selected with an appropriate quantity.</p>";
            var title = "<i class='fe-comment'></i> Cannot Update Work Order!"
            displayDialogMsg(msg, title);
        }
        e.preventDefault()
    });
    // End constructor
    
    //Role Task Details

    $(document).on("click", ".dt-details-role", function (e) {
        var self = $(this);
        var id = self.parents("tr").attr("id");
        loading(false);
        url = self.parents("table").data("roleurl");
        $("body").append("<div class='btm-overlay'></div>");
        $(".btm-overlay").fadeIn(200);
        $.ajax({
            url: url,
            data: { id: id },
            type: "POST",
            success: function (html) {
                removeLoader(false, 500, function () {
                    $("body").append(html);
                    //$("#tasks [title]").tooltip({ placement: "top" });
                    $("#roletask .center").perfectScrollbar();
                });
            }
        });
        e.preventDefault();
    });
    $(document).on("click", "#roletask .tasks-holder td .check-box", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var tds = $("#roletask .tasks-holder td");
        var activeChecks = tds.find(".check-box.active")
        if (tds.find(".check-box").length === activeChecks.length)
            $("#roletask .tasks-holder th .check-box").addClass("active");
        else
            $("#roletask .tasks-holder th .check-box").removeClass("active");        
        e.preventDefault();
    });
    $(document).on("click", "#roletask .tasks-holder th .check-box", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var isActive = self.is(".active");
        if (isActive) {
            $("#roletask .tasks-holder td .check-box").each(function (i, elm) {
                $(elm).addClass("active");
            });
        }
        else
            $("#roletask .tasks-holder td .check-box").removeClass("active");

    });

    $(document).on("click", "#roletask .tasks-holder td .check-box-s", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var tds = $("#roletask .tasks-holder td");
        var activeChecks = tds.find(".check-box-s.active")
        if (tds.find(".check-box-s").length === activeChecks.length)
            $("#roletask .tasks-holder th .check-box-s").addClass("active");
        else
            $("#roletask .tasks-holder th .check-box-s").removeClass("active");        
        e.preventDefault();
    });
    $(document).on("click", "#roletask .tasks-holder th .check-box-s", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var isActive = self.is(".active");
        if (isActive) {
            $("#roletask .tasks-holder td .check-box-s").each(function (i, elm) {
                $(elm).addClass("active");
            });
        }
        else
            $("#roletask .tasks-holder td .check-box-s").removeClass("active");

    });

    $(document).on("click", "#roletask .tasks-holder td .check-box-r", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var tds = $("#roletask .tasks-holder td");
        var activeChecks = tds.find(".check-box-r.active")
        if (tds.find(".check-box-r").length === activeChecks.length)
            $("#roletask .tasks-holder th .check-box-r").addClass("active");
        else
            $("#roletask .tasks-holder th .check-box-r").removeClass("active");
        e.preventDefault();
    });
    $(document).on("click", "#roletask .tasks-holder th .check-box-r", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var isActive = self.is(".active");
        if (isActive) {
            $("#roletask .tasks-holder td .check-box-r").each(function (i, elm) {
                $(elm).addClass("active");
            });
        }
        else
            $("#roletask .tasks-holder td .check-box-r").removeClass("active");

    });

    $(document).on("click", "#roletask .tasks-holder td .check-box-w", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var tds = $("#roletask .tasks-holder td");
        var activeChecks = tds.find(".check-box-w.active")
        if (tds.find(".check-box-w").length === activeChecks.length)
            $("#roletask .tasks-holder th .check-box-w").addClass("active");
        else
            $("#roletask .tasks-holder th .check-box-w").removeClass("active");
        e.preventDefault();
    });
    $(document).on("click", "#roletask .tasks-holder th .check-box-w", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var isActive = self.is(".active");
        if (isActive) {
            $("#roletask .tasks-holder td .check-box-w").each(function (i, elm) {
                $(elm).addClass("active");
            });
        }
        else
            $("#roletask .tasks-holder td .check-box-w").removeClass("active");

    });

    $(document).on("click", "#tasks .close", function (e) {
        $("#tasks").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
            $(this).remove();
            $(".btm-overlay").fadeOut(200, function () { $(this).remove(); });
        });
    });

    // End role task detail


    //metarial constructor
    $(document).on("click", ".mat-const-main .const-holder ul li > .wo-actions .add-materials button", function (e) {
        console.log("asfasf")
        var self = $(this);
        self.toggleClass("off");
        if (self.classList.length > 0) {
            LoadMetarialUsagePopup(self);
        }
        e.preventDefault();
    });
    $(document).on("click", ".has-constructor .dt-details-mat", function (e) {
        var self = $(this);
        var wid = self.parents("tr").attr("id");
        wid = wid.toLowerCase().indexOf("invoiced") > -1 ? "invoiced" : wid.split("_")[1];
        if (wid != "invoiced") {
            loading(false);
            url = self.parents("table").data("maturl");
            $("body").append("<div class='full-overlay'></div>");
            $(".full-overlay").fadeIn(200);
            $.ajax({
                url: url,
                data: { wid: wid },
                type: "POST",
                success: function (html) {
                    removeLoader(false, 500, function () {
                        $("body").append(html);
                        $("#matConstructor [title]").tooltip({ placement: "top" });
                        $("#matConstructor").addClass("animated bounceInRight").show();
                        $('.mat-const-main .const-holder ul').perfectScrollbar();
                        $("#matConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                        $("#matConstructor .title span").dotdotdot();
                        var actHolder = $(".mat-const-main .const-holder.const-mats");
                        if (actHolder.find("li").length == 0) {
                            actHolder.find("ul").remove();
                            $(".mat-const-main .const-mats .placeholder").show(100, function () {
                                $(".mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
                            });
                        }
                        else {
                            var ph = actHolder.find(".placeholder");
                            var time = ph.is(":visible") ? 300 : 10;
                            ph.find("button").removeClass("activated animated flash");
                            ph.hide(200);
                            setTimeout(function () {
                                setDisableMetarialOptions();
                                actHolder.find("select").select2({ width: "resolve" });
                                actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                                $('.mat-const-main .const-mats ul').perfectScrollbar();
                            }, time);
                        }
                    });
                }
            });
        }
        else {
            var htmlMsg = "<p>This Workorder has already been <b>Invoiced</b>. To edit the Workorder you must first reverse the Invoice.</p>";
            var htmlTitle = "<i class='fe-comment'></i>Already Processed";
            displayDialogMsg(htmlMsg, htmlTitle);
        }
        e.preventDefault();
    });
    
    //metarial constructor item actions
    $(document).on("click", "#matConstructor .const-mats > .title > a, #matConstructor .const-mats .placeholder button", function (e) {

        //if ($("#matConstructor .const-locs ul li.active").length == 0) {
        //    var msg = "<p>Please add and/or select a <b>Location</b> before attempting to add any new <b>Activities</b>.</p>";
        //    var title = "<i class='fe-comment'></i> Cannot Add Activity";
        //    displayDialogMsg(msg, title);
        //}
        //else {
            var matsHolder = $("#matConstructor > .mat-const-main .const-mats");
            var self = $(this);
            if (matsHolder.find("ul").length == 0)
                matsHolder.append("<ul></ul>");
            if (matsTempStorage == null) {
                $.ajax({
                    async: false,
                    url: '/workorder/getmetarials',
                    data: { actId: $("#actIdHidden").val() },
                    type: 'post',
                    success: function (r) {
                        matsTempStorage = r;
                    }
                })
            }
            var ph = $("#matConstructor .const-mats .placeholder");
            var time = ph.is(":visible") ? 300 : 10;
            ph.hide(200);
            setTimeout(function () {
                matsHolder.find("ul").append(applyActivityMatTemplate(matsTempStorage));
                $('.mat-const-main .const-mats ul').perfectScrollbar("update");
                setDisableMetarialOptions();
                matsHolder.find("select").select2({ width: "resolve", placeholder: "Select a Metarial" });
                matsHolder.find("input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                //matsHolder.find("input.act-amt").on("blur", function (e) {
                //    var self = $(this);
                //    var prnt = $(this.parentElement);
                //    var id = prnt.find("select").val();
                //    $.post("/workorder/GetMaxMetarialQty", { matId: id }, function (r) {
                //        var msg = "";
                //        var title = "<i class='fe-comment'></i>Quntity Warning!"
                //        if (r.MaxQty != 0) {
                //            if (Number(self.val()) > r.MaxQty)
                //                msg = "Quntity cannot be greater than " + r.MaxQty + ".";
                //        }
                //        if (Number(self.val()) < 1)
                //            msg = "Quntity cannot be 0 or minus value.";
                //        if (msg != "") {
                //            displayDialogMsg(msg, title);
                //            self.val(1.00);
                //        }
                //    });
                //});
            }, time);
        //}
        e.preventDefault();
    });
    $(document).on("change", "#matConstructor .const-mats select", function () {
        var self = $(this);
        var id = self.val();
        setDisableActivityOptions();
        for (var i = 0; i < matsTempStorage.length; i++) {
            if (matsTempStorage[i].activityID == id) {
                var elm = self.parents("li").find(".act-amt");
                $(elm[0]).val(matsTempStorage[i].maxQty);
            }
        }
        //$.get("/workorder/hasmaterial", { id: id }, function (result) {
        //    var parent = self.parents("li").find(".wo-actions");
        //    if (result == true) {
        //        if (parent.find(".mat-action").length < 1) {
        //            parent.append('<a href="#" class="mat-action"><i class="mdi mdi-server"></i></a>');// style="color: green;"
        //        }
        //    }
        //    else {
        //        parent.find(".mat-action").remove();
        //    }
        //});
    });

    $(document).on("blur", "#matConstructor .const-mats input.act-amt", function (e) {
        var self = $(this);
        var prnt = $(this.parentElement);
        var id = prnt.find("select").val();
        var actId = $("#actIdHidden").val();
        $.post("/workorder/GetMaterialMaxQty", { actId: actId, matId: id }, function (r) {
            var msgAddl = "";
            if (r.MaxQty != 0) {
                if (Number(self.val()) > r.MaxQty)
                    msgAddl = "You have exceed the <b class='default-col'>Maximum Quantity ( " + r.MaxQty + " )</b> level for the material.";
            }
            if (Number(self.val()) < 1)
                msgAddl = "You are going to insert <b class='default-col'>Nagavite</b> value as material quantity.";
            if (msgAddl != "") {
                var buttons = [
                        { text: "Continue", click: function () { $(this).dialog("close"); } },
                        { text: "Cancel", click: function () { $(this).dialog("close"); } }
                ];
                var msg = "<div><p>" + msgAddl + " Are you sure you want to Continue?</p></div>";
                displayConfirmationMsg(msg, 'Quantity Warning', buttons);
            }
        });
    });

    //metarial constructor top btns
    $(document).on("click", "#matConstructor > .title .btn-group label:not(.active)", function (e) {
        var self = $(this);
        var selfCls = self.attr("class");
        var type = selfCls.indexOf("unsubmit") > -1 ? "unsubmit" : selfCls.indexOf("verify") > -1 ? "verify" : "submit";
        $.post("/workorder/changeStatus", { type: type, wid: $("#constructor").data("id") }, function (r) {
            if (r.Msg != "") {
                $("#constructor > .title .btn-group label").removeClass("active");
                if (type == "submit")
                    $("#constructor > .title .btn-group label.submit").addClass("active");
                else if (type == "verify")
                    $("#constructor > .title .btn-group label.verify").addClass("active");
                else
                    $("#constructor > .title .btn-group label.unsubmit").addClass("active");
                var msg = r.Msg;
                var title = "<i class='fe-comment'></i> Cannot Change Status!"
                if (r.Code == "1001")
                    title = "<i class='fe-comment'></i>Penalty Warning!"
                displayDialogMsg(msg, title);
            }
        });
    });

    //metarial constructor edit and delete actions    
    $(document).on("click", ".mat-const-main .const-holder ul li .del-action", function (e) {
        var self = $(this);
        temp = self.parents("li");
        if (temp.attr("class").indexOf("new") > -1) {
            temp.remove();
        }
        else {
            var holder = self.parents(".const-holder");
            var type = "Activity";
            var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); deleteMetarial(type); } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msgAddl = "This action will <b class='default-col'>Delete</b> this <b class='act-col'>Metarial</b>."
            var msg = "<div><p>" + msgAddl + " Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Delete ' + type, buttons);
        }
        e.stopPropagation();
        e.preventDefault();
    });
    //metarial constructor bottom btns

    //$(document).on("click", "#matConstructor .process", function (e) {
    //    var id = $("#matConstructor").data("id");
    //    loading(true);
    //    $.get("../workorder/workorderactivitycount", { id: id }, function (data) {
    //        if (!data.any || !data.verified) {
    //            var title = "<i class='fe-comment'></i> Cannot Process Work Order!"
    //            var msg = "";
    //            removeLoader(true, 500, function () {
    //                if (!data.any)
    //                    msg = "<p>Cannot <b class='default-col'>Process</b> a Work Order that has no activities. Please make sure that at least one (1) <b class='act-col'>Activity</b> was created.</p>";
    //                else
    //                    msg = "<p>Cannot <b class='default-col'>Process</b> a Work Order that has not yet been <b class='act-col'>Verified</b>. Please make sure that you have changed the Work Order's status.</p>";
    //                displayDialogMsg(msg, title);
    //            });
    //        }
    //        else {
    //            var invID = $("#constructor .wo-inv select").val();
    //            var title = $("#constructor .title-editor input").val();
    //            var buttons = [
    //                        {
    //                            text: "Continue", click: function () {
    //                                $(this).dialog("close");
    //                                loading(true);
    //                                $.post("../workorder/processactivities", { id: id, invID: invID, title: title }, function (data) {
    //                                    removeLoader(true, 500, function () {
    //                                        if (typeof data.Additional === 'undefined') {
    //                                            $("#constructor").remove();
    //                                            $("body").append(data);
    //                                            $("#constructor").show();
    //                                        }
    //                                        msg = "<p>Successfully <b class='default-col'>Processed</b> Workorder. </p>";
    //                                        var title = "<i class='fe-comment'></i> Success!"
    //                                        displayDialogMsg(msg, title);
    //                                    });
    //                                });
    //                            }
    //                        },
    //                        { text: "Cancel", click: function () { $(this).dialog("close"); } }
    //            ];
    //            var msg = '<p>Are you sure you want to <b class="default-col">Process</b> this Work Order?</p></div>';
    //            removeLoader(true, 500, function () {
    //                displayConfirmationMsg(msg, 'Process Work Order', buttons);
    //            });
    //        }
    //    });
    //    e.preventDefault();
    //});

    $(document).on("click", "#matConstructor .cancel", function (e) {
        var stop = discardMetarialDataCheck("full");
        if (stop) {
            if (!isMatAddedToist) {
                var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); closeMatConstructor(); } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
                ];
                var msg = '<p>Are you sure you want to continue this action. Any unsaved changes will be <b>Discarded</b>!</p></div>';
                displayConfirmationMsg(msg, 'Discard Changes', buttons);
            }
            else
                closeMatConstructor();
        }
        else
            closeMatConstructor();
        e.preventDefault();
    });
    $(document).on("click", "#matConstructor .update", function (e) {
        var mats = [];
        $(".mat-const-main .const-holder.const-mats ul li").each(function () {
            var self = $(this);
            var actID = $("#matConstructor #actIdHidden").val();
            var loc = $("#matConstructor #locHidden").val();
            var date = $("#matConstructor #dateHidden").val();
            var matID = self.find("select").val();
            var matQty = self.find(".act-amt").val();
            if (!matID.isEmpty() && !matQty.isEmpty()) {
                mats.push({
                    actID: actID,
                    loc: loc,
                    date: date,
                    matID: matID,
                    matQty: matQty
                });
            }
        });
        
        if (mats.length > 0) {
            var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); updateMetarial(); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msg = "<p>Only the valid <b class='act-col'>Metarials</b> will be <b class='default-col'>Added to the list </b> metarials will be saved, when <b class='default-col'>saving the activities</b>. Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Update Confirmation', buttons);
        }
        else {
            var msg = "<p>Cannot <b class='default-col'>Update</b> incomplete Work order. Please make sure that at least one (1) <b class='act-col'>Metarial</b> was selected with an appropriate quantity.</p>";
            var title = "<i class='fe-comment'></i> Cannot Update Work Order!"
            displayDialogMsg(msg, title);
        }
        e.preventDefault()
    });
    // End metarial constructor

    //quotation constructor

    //$(document).on("click", ".mat-const-main .const-holder ul li > .wo-actions .add-materials button", function (e) {
    //    console.log("asfasf")
    //    var self = $(this);
    //    self.toggleClass("off");
    //    if (self.classList.length > 0) {
    //        LoadMetarialUsagePopup(self);
    //    }
    //    e.preventDefault();
    //});

    $(document).on("click", ".has-constructor .dt-details-quot", function (e) {
        var self = $(this);
        var qid = self.parents("tr").attr("id");

        loading(false);
        url = self.parents("table").data("qurl");
        $("body").append("<div class='full-overlay'></div>");
        $(".full-overlay").fadeIn(200);
        $.ajax({
            url: url,
            data: { qid: qid },
            type: "POST",
            success: function (html) {
                removeLoader(false, 500, function () {
                    $("body").append(html);
                    $("#qConstructor [title]").tooltip({ placement: "top" });
                    $("#qConstructor").addClass("animated bounceInRight").show();
                    $('#qConstructor .mat-const-main .const-holder ul').perfectScrollbar();
                    $("#qConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                    $("#qConstructor .title span").dotdotdot();
                    var actHolder = $("#qConstructor .mat-const-main .const-holder.const-mats");
                    if (actHolder.find("li").length == 0) {
                        actHolder.find("ul").remove();
                        $("#qConstructor .mat-const-main .const-mats .placeholder").show(100, function () {
                            $("#qConstructor .mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
                        });
                    }
                    else {
                        var ph = actHolder.find(".placeholder");
                        var time = ph.is(":visible") ? 300 : 10;
                        ph.find("button").removeClass("activated animated flash");
                        ph.hide(200);
                        setTimeout(function () {
                            setDisableQuotationActivityOptions();
                            actHolder.find("select").select2({ width: "resolve" });
                            actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                            $('#qConstructor .mat-const-main .const-mats ul').perfectScrollbar();
                        }, time);
                    }
                });
            }
        });
        e.preventDefault();
    });
    
    //quotation constructor item actions
    $(document).on("click", "#qConstructor .const-mats > .title > a, #qConstructor .const-mats .placeholder button", function (e) {
        var matsHolder = $("#qConstructor > .mat-const-main .const-mats");
        var self = $(this);
        if (matsHolder.find("ul").length == 0)
            matsHolder.append("<ul></ul>");
        if (actsTempStorage == null) {
            $.ajax({
                async: false,
                url: '/quotation/GetActivities',
                type: 'post',
                success: function (r) {
                    actsTempStorage = r;
                }
            })
        }
        var ph = $("#qConstructor .const-mats .placeholder");
        var time = ph.is(":visible") ? 300 : 10;
        ph.hide(200);
        setTimeout(function () {
            matsHolder.find("ul").append(applyQuotActivityTemplate(actsTempStorage));
            $('#qConstructor .mat-const-main .const-mats ul').perfectScrollbar("update");
            setDisableQuotationActivityOptions();
            matsHolder.find("select").select2({ width: "resolve", placeholder: "Select a Activity", tags: true });
            matsHolder.find("input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
            matsHolder.find("input.act-amt").on("blur", function (e) {
                var self = $(this);
                var prnt = $(this.parentElement);
                var val = prnt.find("select").val();
                $.post("/quotation/GetMaxQty", { act: val }, function (r) {
                    var msg = "";
                    var title = "<i class='fe-comment'></i>Quntity Warning!"
                    if (r.MaxQty != 0) {
                        if (Number(self.val()) > r.MaxQty)
                            msg = "Quntity cannot be greater than " + r.MaxQty + ".";                        
                    }
                    if (Number(self.val()) < 1)
                        msg = "Quntity cannot be 0 or minus value.";
                    if (msg != "") {
                        displayDialogMsg(msg, title);
                        self.val(1.00);
                    }
                    self.parent("li").find("input.act-rat").val(r.Rate);
                });
            });
        }, time);
        e.preventDefault();
    });
    $(document).on("change", "#qConstructor .const-mats select", function () {
        var self = $(this);
        var val = self.val();
        $.post("/quotation/GetRate", { act: val }, function (r) {
            var prnt = self.parent(".act-holder");
            var elm = prnt.find(".act-rat");
            elm.val(r.Rate);
        });
        setDisableQuotationActivityOptions();
    });
        
    //metarial constructor edit and delete actions    
    $(document).on("click", "#qConstructor .mat-const-main .const-holder ul li .del-action", function (e) {
        var self = $(this);
        temp = self.parents("li");
        if (temp.attr("class").indexOf("new") > -1) {
            temp.remove();
        }
        else {
            var holder = self.parents(".const-holder");
            var type = "Activity";
            var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); deleteQuotationActivity(type); } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msgAddl = "This action will <b class='default-col'>Delete</b> this <b class='act-col'>Activity</b>."
            var msg = "<div><p>" + msgAddl + " Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Delete ' + type, buttons);
        }
        e.stopPropagation();
        e.preventDefault();
    });

    //quotation constructor bottom btns
    $(document).on("click", "#qConstructor .cancel", function (e) {
        var stop = discardQuotationDataCheck("full");
        if (stop) {
            var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); closeQuotConstructor(); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msg = '<p>Are you sure you want to continue this action. Any unsaved changes will be <b>Discarded</b>!</p></div>';
            displayConfirmationMsg(msg, 'Discard Changes', buttons);
        }
        else
            closeQuotConstructor();
        e.preventDefault();
    });
    $(document).on("click", "#qConstructor .update", function (e) {
        var mats = [];
        $("#qConstructor .mat-const-main .const-holder.const-mats ul li").each(function () {
            var self = $(this);
            var matID = self.find("select").val();
            var matQty = self.find(".act-amt").val();
            if (!matID.isEmpty() && !matQty.isEmpty()) {
                mats.push({
                    matID: matID,
                    matQty: matQty
                });
            }
        });

        if (mats.length > 0) {
            var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); updateQuotationActivity(); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msg = "<p>Only the valid <b class='act-col'>Activities</b> will be saved. Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Update Confirmation', buttons);
        }
        else {
            var msg = "<p>Cannot <b class='default-col'>Update</b> incomplete Quotation. Please make sure that at least one (1) <b class='act-col'>Activity</b> was selected with an appropriate quantity.</p>";
            var title = "<i class='fe-comment'></i> Cannot Update Quotation!"
            displayDialogMsg(msg, title);
        }
        e.preventDefault()
    });

    // End quotation constructor


    //Standby Invoice Constructor
    $(document).on("click", ".has-constructor .dt-details-sinv", function (e) {
        var self = $(this);
        var sIid = self.parents("tr").attr("id");

        loading(false);
        url = self.parents("table").data("qurl");
        $("body").append("<div class='full-overlay'></div>");
        $(".full-overlay").fadeIn(200);
        $.ajax({
            url: url,
            data: { sIid: sIid },
            type: "POST",
            success: function (html) {
                removeLoader(false, 500, function () {
                    $("body").append(html);
                    $("#sIConstructor [title]").tooltip({ placement: "top" });
                    $("#sIConstructor").addClass("animated bounceInRight").show();
                    $('#sIConstructor .mat-const-main .const-holder ul').perfectScrollbar();
                    $("#sIConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                    $("#sIConstructor .title span").dotdotdot();
                    var actHolder = $("#sIConstructor .mat-const-main .const-holder.const-mats");
                    if (actHolder.find("li").length == 0) {
                        actHolder.find("ul").remove();
                        $("#sIConstructor .mat-const-main .const-mats .placeholder").show(100, function () {
                            $("#sIConstructor .mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
                        });
                    }
                    else {
                        var ph = actHolder.find(".placeholder");
                        var time = ph.is(":visible") ? 300 : 10;
                        ph.find("button").removeClass("activated animated flash");
                        ph.hide(200);
                        setTimeout(function () {
                            setDisableSInvoiceActivityOptions();
                            actHolder.find("select").select2({ width: "resolve" });
                            actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                            $('#sIConstructor .mat-const-main .const-mats ul').perfectScrollbar();
                        }, time);
                    }
                });
            }
        });
        e.preventDefault();
    });

    //Standby Invoice constructor item actions
    $(document).on("click", "#sIConstructor .const-mats > .title > a, #sIConstructor .const-mats .placeholder button", function (e) {
        var matsHolder = $("#sIConstructor > .mat-const-main .const-mats");
        var self = $(this);
        if (matsHolder.find("ul").length == 0)
            matsHolder.append("<ul></ul>");
        if (actsTempStorage == null) {
            $.ajax({
                async: false,
                url: '/StandbyInvoice/GetActivities',
                type: 'post',
                success: function (r) {
                    actsTempStorage = r;
                }
            })
        }
        var ph = $("#sIConstructor .const-mats .placeholder");
        var time = ph.is(":visible") ? 300 : 10;
        ph.hide(200);
        setTimeout(function () {
            matsHolder.find("ul").append(applySInvActivityTemplate(actsTempStorage));
            $('#sIConstructor .mat-const-main .const-mats ul').perfectScrollbar("update");
            setDisableSInvoiceActivityOptions();
            matsHolder.find("select").select2({ width: "resolve", placeholder: "Select a Activity", tags: true });
            matsHolder.find("input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
            matsHolder.find("input.act-amt").on("blur", function (e) {
                var self = $(this);
                var prnt = $(this.parentElement);
                var val = prnt.find("select").val();
                $.post("/StandbyInvoice/GetMaxQty", { act: val }, function (r) {
                    var msg = "";
                    var title = "<i class='fe-comment'></i>Quntity Warning!"
                    if (r.MaxQty != 0) {
                        if (Number(self.val()) > r.MaxQty)
                            msg = "Quntity cannot be greater than " + r.MaxQty + ".";
                    }
                    if (Number(self.val()) < 1)
                        msg = "Quntity cannot be 0 or minus value.";
                    if (msg != "") {
                        displayDialogMsg(msg, title);
                        self.val(1.00);
                    }
                    self.parent("li").find("input.act-rat").val(r.Rate);
                });
            });
        }, time);
        e.preventDefault();
    });
    $(document).on("change", "#sIConstructor .const-mats select", function () {
        var self = $(this);
        var val = self.val();
        $.post("/StandbyInvoice/GetRate", { act: val }, function (r) {
            var prnt = self.parent(".act-holder");
            var elm = prnt.find(".act-rat");
            elm.val(r.Rate);
        });
        setDisableSInvoiceActivityOptions();
    });
    $(document).on("click", "#sIConstructor .mat-const-main .const-holder ul li .del-action", function (e) {
        var self = $(this);
        temp = self.parents("li");
        if (temp.attr("class").indexOf("new") > -1) {
            temp.remove();
        }
        else {
            var holder = self.parents(".const-holder");
            var type = "Activity";
            var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); deleteSInvoiceActivity(type); } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msgAddl = "This action will <b class='default-col'>Delete</b> this <b class='act-col'>Activity</b>."
            var msg = "<div><p>" + msgAddl + " Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Delete ' + type, buttons);
        }
        e.stopPropagation();
        e.preventDefault();
    });


    //Standby Invoice constructor bottom btns
    $(document).on("click", "#sIConstructor .cancel", function (e) {
        var stop = discardSInvoiceDataCheck("full");
        if (stop) {
            var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); closeSInvoiceConstructor(); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msg = '<p>Are you sure you want to continue this action. Any unsaved changes will be <b>Discarded</b>!</p></div>';
            displayConfirmationMsg(msg, 'Discard Changes', buttons);
        }
        else
            closeSInvoiceConstructor();
        e.preventDefault();
    });
    $(document).on("click", "#sIConstructor .update", function (e) {
        var mats = [];
        $("#sIConstructor .mat-const-main .const-holder.const-mats ul li").each(function () {
            var self = $(this);
            var matID = self.find("select").val();
            var matQty = self.find(".act-amt").val();
            if (!matID.isEmpty() && !matQty.isEmpty()) {
                mats.push({
                    matID: matID,
                    matQty: matQty
                });
            }
        });

        if (mats.length > 0) {
            var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); updateSInvoiceActivity(); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msg = "<p>Only the valid <b class='act-col'>Activities</b> will be saved. Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Update Confirmation', buttons);
        }
        else {
            var msg = "<p>Cannot <b class='default-col'>Update</b> incomplete Standby Invoice. Please make sure that at least one (1) <b class='act-col'>Activity</b> was selected with an appropriate quantity.</p>";
            var title = "<i class='fe-comment'></i> Cannot Update Standby Invoice!"
            displayDialogMsg(msg, title);
        }
        e.preventDefault()
    });

    // End Standby Invoice constructor

    
    $(document).on('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', ".tel-form .msg-bar", function () {
        $(this).removeClass("animated wobble pulse");
    });
    $(document).on("click", ".tel-form .proportion", function (e) {
        $(this).toggleClass("set unset").find("i").toggleClass("fe-infinity-1 fe-waves");
        $(".tel-form .splits").first().trigger("keyup");
        e.preventDefault();
    });
    $(document).on("keyup", ".tel-form .splits", function () {
        if ($(".tel-form .proportion").is(".set")) {
            var self = $(this);
            var otherSplit = self.parents("tr").find(".splits").not(self);
            var splitVal = self.val();
            otherSplit.val(splitVal > 100 ? 0 : 100 - self.val());
        }
    });
    $(document).on("change", '.team-setup .con-ddl', function () {
        var self = $(this);
        var url = self.select2("data")[0].imgUrl;
        var payScal = self.select2("data")[0].payScale;
        var payScaleId = self.select2("data")[0].payScaleId;
        var portion = self.select2("data")[0].Portion;
        var plateNo = self.select2("data")[0].PlateNo;
        var id = self.select2("data")[0].id;
        var tr = self.parents("tr");
        tr.find("img").attr("src", url);
        tr.find("label").text(payScal);
        tr.find("#contractorId").val(id);
        if (tr.find("#memberPayScale") != undefined)
            tr.find("#memberPayScale").val(payScaleId);
        if (document.getElementById("contractorPortion") != undefined)
            $("#contractorPortion").val(portion != undefined && portion != "" ? portion : 100);
        if (plateNo != undefined && plateNo != "") {
            if (document.getElementById("Own1") != undefined)
                $("#Own1").prop("checked", true)
            var plateNoSelect = document.getElementById("plateNoDrop");
            var option = document.createElement("option");
            option.text = plateNo + " - Own Vehicale";
            option.value = plateNo;
            if (plateNoSelect != undefined && plateNoSelect != null)
                plateNoSelect.add(option, 0);
            $("#plateNoDrop").val(plateNo);
            if (tr.find("#memberPlateNo") != undefined)
                tr.find("#memberPlateNo").val(plateNo);
        }
        if (document.getElementById("techNoLbl") != undefined) {

        }
        tr.data("id", self.val());
    });
    $(document).on("click", ".tel-form .team-setup .add-new-mem", function (e) {
        if (($(".tel-form .team-setup tr").length - 1) < parseInt($(".tel-form .team-setup").data("team"))) {
            $.get("workorder/getcontractorpartial", function (html) {
                $(".tel-form .team-setup").append(html);
                setupTeamActions(false);
            });
        }
        e.preventDefault();
    });
    $(document).on("click", ".tel-form .team-setup .remove-mem", function (e) {
        $(this).parents("tr").remove();
        e.preventDefault();
    });
    $(document).on("click", ".team-btn", function (e) {
        SubmitTeam(false);
        e.stopPropagation();
        e.preventDefault();
    });
    function SubmitTeam(fromDetail) {
        var team = [];
        var ok = false;
        $(".tel-form .team-setup tr.team-mem").each(function () {
            var tr = $(this);
            var id = tr.data("id").toString();
            var share = tr.find(".portion").val();
            var isLead = tr.find(".lead").val();
            if (!id.isEmpty() && !share.isEmpty()) {
                team.push({
                    id: id,
                    share: share,
                    islead: isLead.toLowerCase() == "false" ? false : true
                });
                ok = true;
            }
            else {
                ok = false;
                return false;
            }
        });
        var anyleader = team.filter(function (elm) {
            return elm.islead == true;
        });
        if (ok && anyleader.length > 0) {
            var data = {
                wid: $(".tel-form-id").val(),
                team: team
            };
            $.post("workorder/saveteam", $.toDictionary({ model: data }), function (data) {
                if (data.Code != "100")
                    setupMsgBar(3, "Create/Update of Team failed. Please try again or contact your system administrator");
                else {
                    setupMsgBar(2, "Team was Saved/Update successfully!");
                    var page = dTable.page();
                    dTable.page(page).draw(false);
                    //dTable.draw();
                    if (!fromDetail) {
                        SubmitWO(self, true);
                    }
                    setTimeout('$(".close").click()', 2000);
                }
            });
        }
        else
            setupMsgBar(3, "Saving team failed. Please ensure that all inputs were entered and that a leader was selected.");        
    }
    //function SaveWO() {
    //    var form = $(".tel-form form");

    //    var team = [];
    //    var ok = false;
    //    $(".tel-form .team-setup tr.team-mem").each(function () {
    //        var tr = $(this);
    //        var id = tr.data("id").toString();
    //        var share = tr.find(".portion").val();
    //        var isLead = tr.find(".lead").val();
    //        if (!id.isEmpty() && !share.isEmpty()) {
    //            team.push({
    //                id: id,
    //                share: share,
    //                islead: isLead.toLowerCase() == "false" ? false : true
    //            });
    //            ok = true;
    //        }
    //        else {
    //            ok = false;
    //            return false;
    //        }
    //    });
    //    if (team.length > 0) {
    //        var anyleader = team.filter(function (elm) {
    //            return elm.islead == true;
    //        });
    //        if (ok && anyleader.length > 0) {
    //            var data = {
    //                wid: $(".tel-form-id").val(),
    //                team: team
    //            };
    //            $.post("workorder/saveteam", $.toDictionary({ model: data }), function (data) {
    //                if (data.Code != "100")
    //                    setupMsgBar(3, "Create/Update of Team failed. Please try again or contact your system administrator");
    //                else
    //                    setupMsgBar(2, "Team was Saved/Update successfully!");
    //            });
    //        }
    //        else
    //            setupMsgBar(3, "Saving team failed. Please ensure that all inputs were entered and that a leader was selected.");
    //        e.stopPropagation();
    //    }
    //}
    $(document).on("click", ".merged-singular ", function (e) {
        var self = $(this);
        self.toggleClass("merged");
        $(".merge-adder").toggle(200);
        self.find("i").toggleClass("fe-shuffle fe-list")
        $(".slider.active").hide("slide", { direction: "right" }, 200, function () {
            var $this = $(this);
            $(".slider").not($this).show("slide", { direction: "left" }, 200).addClass("active")
        }).removeClass("active");
    });
    $(document).on("click", ".merge-adder", function (e) {
        loading(false);
        $.get("workorder/Merger", function (html) {
            $("body").append("<div class='btm-overlay'></div>");
            $(".btm-overlay").fadeIn(200);
            removeLoader(false, 500, function () {
                $("body").append(html);
                $("#merger-left ul, #merger-right .center").perfectScrollbar();
            });
        });
    });
    $(document).on("click", ".dt-edit-merge", function (e) {
        var self = $(this);
        var row = self.parents("tr");
        var id = $(this).parents("tr").attr("id");
        id = id.indexOf("_") > -1 ? id.split("_")[1] : id;
        loading(false);
        $.get("workorder/GetMerger", { id: id }, function (html) {
            $("body").append("<div class='btm-overlay'></div>");
            $(".btm-overlay").fadeIn(200);
            removeLoader(false, 500, function () {
                $("body").append(html);
                $("#merger-left ul, #merger-right .center").perfectScrollbar();
            });
        });
        e.preventDefault();
    });
    $(document).on("click", ".dt-delete-merge", function (e) {
        var self = $(this);
        var itemName = self.parents("tr").find(".dt-item-refnum").text();
        var itemType = self.parents("table").data("name");
        var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); deleteMergeRow(self); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
        ];
        var msg = '<p>Are you sure you want to <b class="default-col">Delete</b> <b>' + itemName + '</b> record?</p></div>';
        displayConfirmationMsg(msg, 'Delete ' + itemType, buttons);
        e.preventDefault();
    });
    $(document).on("keyup", "#merger-left .filter input", function () {
        var val = $(this).val();
        var holder = $("#merger-left ul");
        $("#merger-left ul").perfectScrollbar('destroy');
        $.get("workorder/GetMergeReferences", { val: val }, function (data) {
            holder.html(data);
            $("#merger-left ul").perfectScrollbar();
        });
    });
    $(document).on("click", "#merger-left ul li:not(.empty)", function () {        
        var self = $(this);
        if ($("#mergerIdHidden").val() != "" && $("#mergerIdHidden").val() != undefined) {
            LoadMergedGridWithMerged(self, $("#mergerIdHidden").val());
            return;
        }
        $("#merger-left ul li:not(.empty)").removeClass("active");
        var val = self.data("code");
        self.addClass("active");
        var holder = $("#merger-right .workorder-holder");
        $("#merger-right .center").perfectScrollbar('destroy');
        $.get("workorder/MergeCandiditates", { code: val }, function (data) {
            holder.html(data);
            $("#merger-right .total.tot").data("total", 0).find("span").text("$0.00");
            $("#merger-right .center").perfectScrollbar();
            CalculateTotal();
        });
    });
    $(document).on("click", "#merger-right .workorder-holder td .check-box", function (e) {
        var self = $(this);
        var totHolder = $("#merger-right .total.tot");
        totHolder.removeClass("pulse");
        self.toggleClass("active");
        var total = parseFloat(self.parents("tr").find("td:last-child").data("total"));
        total *= self.is(".active") ? 1 : -1;
        var oldTotal = parseFloat(totHolder.data("total"));
        var newTotal = total + oldTotal;
        totHolder.find("span").text(newTotal.formatMoney(2, "$"));
        totHolder.data("total", newTotal).addClass("pulse");
        var tds = $("#merger-right .workorder-holder td");
        var activeChecks = tds.find(".check-box.active")
        if (tds.find(".check-box").length === activeChecks.length)
            $("#merger-right .workorder-holder th .check-box").addClass("active");
        else
            $("#merger-right .workorder-holder th .check-box").removeClass("active");
        if (activeChecks.length > 1)
            $("#merger-right .merger-btn").show(200);
        else
            $("#merger-right .merger-btn").hide(200);
        e.preventDefault();
    });
    $(document).on("click", "#merger-right .workorder-holder th .check-box", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var isActive = self.is(".active");
        var total = 0;
        if (isActive) {
            $("#merger-right .workorder-holder td .check-box").each(function (i, elm) {
                $(elm).addClass("active");
                total += parseFloat($(elm).parents("tr").find("td:last-child").data("total"));
            });
        }
        else
            $("#merger-right .workorder-holder td .check-box").removeClass("active");

        var totHolder = $("#merger-right .total.tot");
        totHolder.removeClass("pulse");
        totHolder.find("span").text(total.formatMoney(2, "$"));
        totHolder.data("total", total).addClass("pulse");
        if (isActive)
            $("#merger-right .merger-btn").show(200);
        else
            $("#merger-right .merger-btn").hide(200);
    });
    $(document).on("click", "#merger .close", function (e) {
        $("#merger").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
            $(this).remove();
            $(".btm-overlay").fadeOut(200, function () { $(this).remove(); });
        });
    });
    $(document).on("click", "#merger .merger-process", function (e) {
        var code = $("#merger-left ul li.active").data("code");
        if (code != undefined && code != "") {
            loading(true);
            $.get("../workorder/mergeractivitycount", { id: $("#mergerIdHidden").val() }, function (data) {
                if (!data.any || !data.verified) {
                    var title = "<i class='fe-comment'></i> Cannot Process!"
                    var msg = "";
                    removeLoader(true, 500, function () {
                        if (data.msg == "") {
                            if (!data.any)
                                msg = "<p>Cannot <b class='default-col'>Process</b> the Merged Work Orders those have no activities. Please make sure that at least one (1) <b class='act-col'>Activity</b> was created.</p>";
                            else
                                msg = "<p>Cannot <b class='default-col'>Process</b> the Merged Work Orders those have not yet been <b class='act-col'>Verified</b>. Please make sure that you have changed the Work Order's status.</p>";
                        }
                        else
                            msg = "<p>Cannot <b class='default-col'>Process</b>. Please make sure that you have <b class='default-col'>Merged</b> the Work Orders.</p>";
                        displayDialogMsg(msg, title);
                    });
                }
                else {
                    var invID = $("#merger select").val();
                    var buttons = [
                                {
                                    text: "Continue", click: function () {
                                        $(this).dialog("close");
                                        loading(true);
                                        $.post("../workorder/processmerger", { code: code, invID: invID, id: $("#mergerIdHidden").val(), title: $("#txtTitle").val() }, function (data) {
                                            removeLoader(true, 500, function () {
                                                if (typeof data.Additional === 'undefined') {
                                                    $("#merger").remove();
                                                    $("body").append(data);
                                                    $("#merger").show();
                                                }
                                                msg = "<p>Successfully <b class='default-col'>Processed</b> Work Orders. </p>";
                                                var title = "<i class='fe-comment'></i> Success!"
                                                displayDialogMsg(msg, title);
                                            });
                                        });
                                    }
                                },
                                { text: "Cancel", click: function () { $(this).dialog("close"); } }
                    ];
                    var msg = '<p>Are you sure you want to <b class="default-col">Process</b> this Merged Work Orders?</p></div>';
                    removeLoader(true, 500, function () {
                        displayConfirmationMsg(msg, 'Process Merged Work Orders', buttons);
                    });
                }
            });
        }
        else {
            var title = "<i class='fe-comment'></i> Cannot Process!"
            var msg = "<p>Cannot <b class='default-col'>Process</b>. Please make sure that you have selected the <b class='default-col'>Merged</b> Work Orders.</p>";
            displayDialogMsg(msg, title);
        }
        e.preventDefault();
    });

    /*New invoices*/

    $(document).on("click", ".inv-singular ", function (e) {
        var self = $(this);
        self.toggleClass("old-invoice");
        $(".inv-adder").toggle(200);
        self.find("i").toggleClass("fe-shuffle fe-list")
        $(".slider.active").hide("slide", { direction: "right" }, 200, function () {
            var $this = $(this);
            $(".slider").not($this).show("slide", { direction: "left" }, 200).addClass("active")
        }).removeClass("active");
    });
    $(document).on("click", ".inv-adder", function (e) {
        loading(false);
        $.get("invoice/newinv", function (html) {
            $("body").append("<div class='btm-overlay'></div>");
            $(".btm-overlay").fadeIn(200);
            removeLoader(false, 500, function () {
                $("body").append(html);
                $("#inv-left ul, #inv-right .center").perfectScrollbar();
            });
        });
    });
    $(document).on("click", ".dt-edit-merge", function (e) {
        var self = $(this);
        var row = self.parents("tr");
        var id = $(this).parents("tr").attr("id");
        id = id.indexOf("_") > -1 ? id.split("_")[1] : id;
        loading(false);
        $.get("invoice/GetNewInvoiceCreator", { id: id }, function (html) {
            $("body").append("<div class='btm-overlay'></div>");
            $(".btm-overlay").fadeIn(200);
            removeLoader(false, 500, function () {
                $("body").append(html);
                $("#inv-left ul, #inv-right .center").perfectScrollbar();
            });
        });
        e.preventDefault();
    });
    $(document).on("click", ".dt-delete-inv", function (e) {
        var self = $(this);
        var itemName = self.parents("tr").find(".dt-item-refnum").text();
        var itemType = self.parents("table").data("name");
        var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); deleteInvRow(self); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
        ];
        var msg = '<p>Are you sure you want to <b class="default-col">Delete</b> <b>' + itemName + '</b> record?</p></div>';
        displayConfirmationMsg(msg, 'Delete ' + itemType, buttons);
        e.preventDefault();
    });
    //$(document).on("click", "#inv-left .inv-bot button", function () {
    //    debugger;
    //    var val = $("#inv-left .filter input").val();
    //    var holder = $("#inv-left ul");
    //    $("#inv-left ul").perfectScrollbar('destroy');
    //    $.get("invoice/GetMergeReferences", { val: val }, function (data) {
    //        holder.html(data);
    //        $("#inv-left ul").perfectScrollbar();
    //    });
    //});
    $(document).on("click", "#inv-left .inv-bot button", function () {
        var form = $("#inv-left form");
        var dateFrom = $("#inv-left [name='dateFrom']").val();
        var dateTo = $("#inv-left [name='dateTo']").val();
        //var self = $(this);
        var formdata = {
            client: $("#client").val(),
            po: $("#po").val(),
            classifi: $("#classifi").val(),
            region: $("#region").val(),
            dateFrom: dateFrom,
            dateTo: dateTo
        };
        if ($("#invIdHidden").val() != "" && $("#invIdHidden").val() != undefined) {
            LoadInvGridWithInv(formdata, $("#invIdHidden").val());
            return;
        }
        //$("#inv-left ul li:not(.empty)").removeClass("active");
        //var val = self.data("code");
        //self.addClass("active");
        var holder = $("#inv-right .workorder-holder");
        $("#inv-right .center").perfectScrollbar('destroy');
        $.post("invoice/InvoiceCandiditates", { model: formdata }, function (data) {
            holder.html(data);
            $("#inv-right .total.tot").data("total", 0).find("span").text("$0.00");
            $("#inv-right .center").perfectScrollbar();
            CalculateWOTotal();
        });
    });
    $(document).on("click", "#inv-left ul li:not(.empty)", function () {
        var self = $(this);
        if ($("#invIdHidden").val() != "" && $("#invIdHidden").val() != undefined) {
            LoadInvGridWithInv(self, $("#invIdHidden").val());
            return;
        }
        $("#inv-left ul li:not(.empty)").removeClass("active");
        var val = self.data("code");
        self.addClass("active");
        var holder = $("#inv-right .workorder-holder");
        $("#inv-right .center").perfectScrollbar('destroy');
        $.get("invoice/MergeCandiditates", { code: val }, function (data) {
            holder.html(data);
            $("#inv-right .total.tot").data("total", 0).find("span").text("$0.00");
            $("#inv-right .center").perfectScrollbar();
            CalculateWOTotal();
        });
    });
    $(document).on("click", "#inv-right .workorder-holder td .check-box", function (e) {
        var self = $(this);
        var totHolder = $("#inv-right .total.tot");
        totHolder.removeClass("pulse");
        self.toggleClass("active");
        var total = parseFloat(self.parents("tr").find("td:last-child").data("total"));
        total *= self.is(".active") ? 1 : -1;
        var oldTotal = parseFloat(totHolder.data("total"));
        var newTotal = total + oldTotal;
        totHolder.find("span").text(newTotal.formatMoney(2, "$"));
        totHolder.data("total", newTotal).addClass("pulse");
        var tds = $("#inv-right .workorder-holder td");
        var activeChecks = tds.find(".check-box.active")
        if (tds.find(".check-box").length === activeChecks.length)
            $("#inv-right .workorder-holder th .check-box").addClass("active");
        else
            $("#inv-right .workorder-holder th .check-box").removeClass("active");
        if (activeChecks.length > 1)
            $("#inv-right .inv-btn").show(200);
        else
            $("#inv-right .inv-btn").hide(200);
        e.preventDefault();
    });
    $(document).on("click", "#inv-right .workorder-holder th .check-box", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var isActive = self.is(".active");
        var total = 0;
        if (isActive) {
            $("#inv-right .workorder-holder td .check-box").each(function (i, elm) {
                $(elm).addClass("active");
                total += parseFloat($(elm).parents("tr").find("td:last-child").data("total"));
            });
        }
        else
            $("#inv-right .workorder-holder td .check-box").removeClass("active");

        var totHolder = $("#inv-right .total.tot");
        totHolder.removeClass("pulse");
        totHolder.find("span").text(total.formatMoney(2, "$"));
        totHolder.data("total", total).addClass("pulse");
        if (isActive)
            $("#inv-right .inv-btn").show(200);
        else
            $("#inv-right .inv-btn").hide(200);
    });
    $(document).on("click", "#inv .close", function (e) {
        $("#inv").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
            $(this).remove();
            $(".btm-overlay").fadeOut(200, function () { $(this).remove(); });
        });
    });
    //$(document).on("click", "#inv .inv-process", function (e) {
    function NewInvoiceProcess(ids, masterId, dBox) {

        var code = $("#inv-left ul li.active").data("code");
        if (ids.length > 0) {
            loading(true);
            $.post("../invoice/CheckForValidWOs", { ids: ids }, function (data) {
                if (!data.any || !data.verified) {
                    var title = "<i class='fe-comment'></i> Cannot Process!"
                    var msg = "";
                    removeLoader(true, 500, function () {
                        if (data.msg == "") {
                            if (!data.any)
                                msg = "<p>Cannot <b class='default-col'>Process</b> the Merged Work Orders those have no activities. Please make sure that at least one (1) <b class='act-col'>Activity</b> was created.</p>";
                            else
                                msg = "<p>Cannot <b class='default-col'>Process</b> the Merged Work Orders those have not yet been <b class='act-col'>Verified</b>. Please make sure that you have changed the Work Order's status.</p>";
                        }
                        else
                            msg = "<p>Cannot <b class='default-col'>Process</b>. Please make sure that you have already <b class='default-col'>Processed</b> the Work Orders.</p>";
                        displayDialogMsg(msg, title);
                    });
                }
                else {
                    var invID = $("#inv .bottom select").val();
                    //var buttons = [
                    //            {
                    //                text: "Continue", click: function () {
                    //                    $(this).dialog("close");
                    //                    Saveinv(ids, invID, dBox);
                    //                    //loading(true);
                    //                    //$.post("../invoice/ProcessWorkOrders", { code: code, invID: invID, id: $("#invIdHidden").val(), title: $("#txtTitle").val() }, function (data) {
                    //                    //    removeLoader(true, 500, function () {
                    //                    //        if (typeof data.Additional === 'undefined') {
                    //                    //            $("#inv").remove();
                    //                    //            $("body").append(data);
                    //                    //            $("#inv").show();
                    //                    //        }
                    //                    //        msg = "<p>Successfully <b class='default-col'>Processed</b> Work Orders. </p>";
                    //                    //        var title = "<i class='fe-comment'></i> Success!"
                    //                    //        displayDialogMsg(msg, title);
                    //                    //    });
                    //                    //});
                    //                }
                    //            },
                    //            { text: "Cancel", click: function () { $(this).dialog("close"); } }
                    //];
                    //var msg = '<p>Are you sure you want to <b class="default-col">Process</b> this Selected Work Orders?</p></div>';
                    //removeLoader(true, 500, function () {
                    //    displayConfirmationMsg(msg, 'Process Selected Work Orders', buttons);
                    //});
                    Saveinv(ids, invID, dBox);
                }
            });
        }
        else {
            var title = "<i class='fe-comment'></i> Cannot Process!"
            var msg = "<p>Cannot <b class='default-col'>Process</b>. Please make sure that you have selected the Work Orders.</p>";
            displayDialogMsg(msg, title);
        }
       // e.preventDefault();
    }

    $(document).on("click", ".inv-process", function (e) {
        var rows = $(".workorder-holder table > tbody  > tr");
        if (rows.length < 1) {
            var htmlMsg = "<p>No <b class='loc-col'>Work Order(s)</b> have been selected for <b class='date-col'>Process</b>. First select your desired <b class='loc-col'>Work Order(s)</b> then try again.</p>";
            var htmlTitle = "<i class='fe-comment'></i>Cannot Process";
            displayDialogMsg(htmlMsg, htmlTitle);
        }
        else {
            rows = $.grep(rows, function (row) {
                var self = $(row).find("button");
                var isChecked = $(self).is(".active");
                return isChecked;
            });
            var ids = $.map(rows, function (elm) { return $(elm).attr("id").replace("tr-", ""); });
            var masterId = $("#invIdHidden").val();//Math.min.apply(Math, ids);
            //ids = $.grep(ids, function (id) {
            //    return id != masterId;
            //});
            if (ids.length > 0) {
                var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); NewInvoiceProcess(ids, masterId, $(this)); } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
                ];
                var msg = "<p>Selected <b class='date-col'>Work Orders</b>, will be <b class='default-col'>Process</b>. Are you sure you want to Continue?</p></div>";
                displayConfirmationMsg(msg, 'Process Confirmation', buttons);
            }
            else {
                var htmlMsg = "<p>No <b class='loc-col'>Work Order(s)</b> have been selected for <b class='date-col'>Process</b>. First select your desired <b class='loc-col'>Work Order(s)</b> then try again.</p>";
                var htmlTitle = "<i class='fe-comment'></i>Cannot Process";
                displayDialogMsg(htmlMsg, htmlTitle);
            }
        }
        e.preventDefault();
    });
    function Saveinv(ids, masterId, dBox) {        
        if ($("#txtTitle").val() == "") {
            removeLoader(true, 200, function () {
                var htmlMsg = "<p>No <b class='loc-col'>Invoice Title</b> have been inserted for <b class='date-col'>Process</b>. First insert your desired <b class='loc-col'>Title</b> then try again.</p>";
                var htmlTitle = "<i class='fe-comment'></i>Cannot Process";
                displayDialogMsg(htmlMsg, htmlTitle);
            });
        }
        else {
            loading(true);
            var isUpdate = $("#invIdHidden").val() != undefined && $("#invIdHidden").val() != "" && $("#invIdHidden").val() != "" ? true : false;
            $.ajax({
                url: "../invoice/ProcessWorkOrders",
                data: $.toDictionary({ wOIds: ids, isUpdate: isUpdate, id: masterId, title: $("#txtTitle").val() }),
                type: "post",
                success: function (r) {
                    removeLoader(true, 200, function () {
                        //$("#inv").remove();
                        //$("body").append(data);
                        //$("#inv").show();
                        var htmlMsg = "";
                        var htmlTitle = "";
                        if (r.Code == "100") {
                            //$("#invIdHidden").val(r.Additional);
                            //htmlMsg = "<p>Work Orders were <b>Processed</b> Successfully!.</p>";
                            //htmlTitle = "<i class='fe-comment'></i> Success";
                            dTable2.draw();
                            $("#inv").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
                                $(this).remove();
                                $(".btm-overlay").fadeOut(200, function () { $(this).remove(); });
                            });                            
                        }
                        else {
                            htmlMsg = r.Msg;
                            htmlTitle = "<i class='fe-comment'></i>Cannot Process";
                            displayDialogMsg(htmlMsg, htmlTitle);
                        }                        
                    });
                    try {
                        dBox.dialog("close");
                    }
                    catch (e) {

                    }
                }
            });
        }
    }
    /*End New invoices*/

});
function LoadMetarialUsagePopup(self) {
    var wid = self.parents("tr").attr("id");
    //wid = wid.toLowerCase().indexOf("invoiced") > -1 ? "invoiced" : wid.split("_")[1];
    //if (wid != "invoiced") {
    //    loading(false);
    //    url = self.parents("table").data("maturl");
    //    $("body").append("<div class='full-overlay'></div>");
    //    $(".full-overlay").fadeIn(200);
    //    $.ajax({
    //        url: url,
    //        data: { wid: wid },
    //        type: "POST",
    //        success: function (html) {
    //            removeLoader(false, 500, function () {
    //                $("body").append(html);
    //                $("#matConstructor [title]").tooltip({ placement: "top" });
    //                $("#matConstructor").addClass("animated bounceInRight").show();
    //                $('.mat-const-main .const-holder ul').perfectScrollbar();
    //                $("#matConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
    //                $("#matConstructor .title span").dotdotdot();
    //                var actHolder = $(".mat-const-main .const-holder.const-mats");
    //                if (actHolder.find("li").length == 0) {
    //                    actHolder.find("ul").remove();
    //                    $(".mat-const-main .const-mats .placeholder").show(100, function () {
    //                        $(".mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
    //                    });
    //                }
    //                else {
    //                    var ph = actHolder.find(".placeholder");
    //                    var time = ph.is(":visible") ? 300 : 10;
    //                    ph.find("button").removeClass("activated animated flash");
    //                    ph.hide(200);
    //                    setTimeout(function () {
    //                        setDisableMetarialOptions();
    //                        actHolder.find("select").select2({ width: "resolve" });
    //                        actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
    //                        $('.mat-const-main .const-mats ul').perfectScrollbar();
    //                    }, time);
    //                }
    //            });
    //        }
    //    });
    //}
    //else {
    //    var htmlMsg = "<p>This Workorder has already been <b>Invoiced</b>. To edit the Workorder you must first reverse the Invoice.</p>";
    //    var htmlTitle = "<i class='fe-comment'></i>Already Processed";
    //    displayDialogMsg(htmlMsg, htmlTitle);
    //}
}


function LoadMergeWOs(code, id) {
    var self = null;
    var val = code;
    var holder = $("#merger-left ul");
    $("#merger-left ul").perfectScrollbar('destroy');
    $.get("workorder/GetMergeReferencesForMerged", { val: val, id: id }, function (data) {
        holder.html(data);
        $("#merger-left ul").perfectScrollbar();
        var items = $("#merger-left ul li");
        for (var i = 0; i < items.length; i++) {
            if ($(items[i]).data("code").toLowerCase() == code.toLowerCase()) {
                self = $(items[i]);
                break;
            }
        }
        LoadMergedGridWithMerged(self, id);
    });
}
function LoadMergedGridWithMerged(self, id) {
    $("#merger-left ul li:not(.empty)").removeClass("active");
    var val = self.data("code");
    self.addClass("active");
    var holder = $("#merger-right .workorder-holder");
    $("#merger-right .center").perfectScrollbar('destroy');
    $.get("workorder/MergeCandiditatesWithMerged", { code: val, id: id }, function (data) {
        holder.html(data);
        $("#merger-right .total.tot").data("total", 0).find("span").text("$0.00");
        $("#merger-right .center").perfectScrollbar();
        CalculateTotal();
    });
}
function LoadMergedGrid(self) {
    $("#merger-left ul li:not(.empty)").removeClass("active");
    var val = self.data("code");
    self.addClass("active");
    var holder = $("#merger-right .workorder-holder");
    $("#merger-right .center").perfectScrollbar('destroy');
    $.get("workorder/MergeCandiditates", { code: val }, function (data) {
        holder.html(data);
        $("#merger-right .total.tot").data("total", 0).find("span").text("$0.00");
        $("#merger-right .center").perfectScrollbar();
        CalculateTotal();
    });
}
function CalculateTotal() {
    var totHolder = $("#merger-right .total.tot");
    totHolder.removeClass("pulse");
    var total = 0;
    $('.workorder > tbody  > tr').each(function () {
        var row = $(this);
        if(row.find(".check-box").is(".active"))
            total += parseFloat(row.find("td:last-child").data("total"));
    });
    totHolder.find("span").text(total.formatMoney(2, "$"));
    totHolder.data("total", total).addClass("pulse");
    var tds = $("#merger-right .workorder-holder td");
    var activeChecks = tds.find(".check-box.active")
    if (tds.find(".check-box").length === activeChecks.length)
        $("#merger-right .workorder-holder th .check-box").addClass("active");
    else
        $("#merger-right .workorder-holder th .check-box").removeClass("active");
    if (activeChecks.length > 1)
        $("#merger-right .merger-btn").show(200);
    else
        $("#merger-right .merger-btn").hide(200);
}
function deleteMergeRow(elm) {
    var url = "workorder/DeleteMerge";
    loading(true);
    $.post(url, { id: elm.parents("tr").attr("id") }, function (data) {
        removeLoader(true, 300, function () {
            if (data.Additional > 0) {
                $(".tel-confirm-dialog").dialog("close");
                var page = dTable2.page();
                dTable2.page(page).draw(false);
                //dTable2.draw();
            }
            else {
                $(".tel-confirm-dialog").dialog("close");
                var htmlMsg = "<p>This record cannot be <b class='default-col'>Deleted</b> because it is still <b>Linked</b> to other records in the Database.</p>";
                var htmlTitle = "<i class='fe-comment'></i>Cannot Delete";
                displayDialogMsg(htmlMsg, htmlTitle);
            }
        });
    });
}

function LoadInvWOs(code, id) {
    var self = null;
    var val = code;
    var holder = $("#inv-left ul");
    $("#inv-left ul").perfectScrollbar('destroy');
    $.get("invoice/GetMergeReferencesForMerged", { val: val, id: id }, function (data) {
        holder.html(data);
        $("#inv-left ul").perfectScrollbar();
        var items = $("#inv-left ul li");
        for (var i = 0; i < items.length; i++) {
            if ($(items[i]).data("code").toLowerCase() == code.toLowerCase()) {
                self = $(items[i]);
                break;
            }
        }
        LoadInvGridWithInv(self, id);
    });
}
function LoadInvGridWithInv(data, id) {
    //$("#inv-left ul li:not(.empty)").removeClass("active");
    //var val = self.data("code");
    //self.addClass("active");
    var holder = $("#inv-right .workorder-holder");
    $("#inv-right .center").perfectScrollbar('destroy');

    $.get("invoice/InvoiceCandiditatesWithInvoiced", { model: data, id: id }, function (data) {
        holder.html(data);
        $("#inv-right .total.tot").data("total", 0).find("span").text("$0.00");
        $("#inv-right .center").perfectScrollbar();
        CalculateWOTotal();
    });
}
function LoadInvGrid(self) {
    $("#inv-left ul li:not(.empty)").removeClass("active");
    var val = self.data("code");
    self.addClass("active");
    var holder = $("#inv-right .workorder-holder");
    $("#inv-right .center").perfectScrollbar('destroy');
    $.get("invoice/MergeCandiditates", { code: val }, function (data) {
        holder.html(data);
        $("#inv-right .total.tot").data("total", 0).find("span").text("$0.00");
        $("#inv-right .center").perfectScrollbar();
        CalculateWOTotal();
    });
}
function CalculateWOTotal() {
    var totHolder = $("#inv-right .total.tot");
    totHolder.removeClass("pulse");
    var total = 0;
    $('.workorder > tbody  > tr').each(function () {
        var row = $(this);
        if (row.find(".check-box").is(".active"))
            total += parseFloat(row.find("td:last-child").data("total"));
    });
    totHolder.find("span").text(total.formatMoney(2, "$"));
    totHolder.data("total", total).addClass("pulse");
    var tds = $("#inv-right .workorder-holder td");
    var activeChecks = tds.find(".check-box.active")
    if (tds.find(".check-box").length === activeChecks.length)
        $("#inv-right .workorder-holder th .check-box").addClass("active");
    else
        $("#inv-right .workorder-holder th .check-box").removeClass("active");
    if (activeChecks.length > 1)
        $("#inv-right .inv-btn").show(200);
    else
        $("#inv-right .inv-btn").hide(200);
}
function deleteinvow(elm) {
    var url = "invoice/DeleteMerge";
    loading(true);
    $.post(url, { id: elm.parents("tr").attr("id") }, function (data) {
        removeLoader(true, 300, function () {
            if (data.Additional > 0) {
                $(".tel-confirm-dialog").dialog("close");
                var page = dTable2.page();
                dTable2.page(page).draw(false);
                //dTable2.draw();
            }
            else {
                $(".tel-confirm-dialog").dialog("close");
                var htmlMsg = "<p>This record cannot be <b class='default-col'>Deleted</b> because it is still <b>Linked</b> to other records in the Database.</p>";
                var htmlTitle = "<i class='fe-comment'></i>Cannot Delete";
                displayDialogMsg(htmlMsg, htmlTitle);
            }
        });
    });
}


function setValidationMsgsForForm(form) {
    form.find(".field-validation-error").each(function () {
        var self = $(this);
        var msg = self.find("span").text();
        if (self.parents("td").length > 0)
            self.parents("td").find("input, select").attr("title", msg).tooltip({ placement: "top" });
        else
            self.parent().find("input, select").attr("title", msg).tooltip({ placement: "top" });
    });
}
function setValidationMsgsForElm(elm) {
    var parent = elm.parents("td").length == 0 ? elm.parent() : elm.parents("td");
    var msg = parent.find(".field-validation-error").find("span").text();
    elm.attr("title", msg).tooltip({ placement: "top" });
}

function deleteSWORow(elm) {
    var url = deleteUrl == null ? elm.parents("table").data("url") + "/deleteSWO" : deleteUrl;
    loading(true);
    $.post(url, { id: elm.parents("tr").attr("id") }, function (data) {
        removeLoader(true, 300, function () {
            if (data.Additional > 0) {
                $(".tel-confirm-dialog").dialog("close");
                var page = dTable.page();
                dTable.page(page).draw(false);
                //dTable.draw();
            }
            else {
                $(".tel-confirm-dialog").dialog("close");
                var htmlMsg = "<p>This record cannot be <b class='default-col'>Deleted</b> because it is <b>not in</b> New status.</p>";
                var htmlTitle = "<i class='fe-comment'></i>Cannot Delete";
                displayDialogMsg(htmlMsg, htmlTitle);
            }
        });
    });
}
function displayDialogMsg(htmlMsg, htmlTitle) {
    var html = '<div class="tel-dialog" title="' + htmlTitle + '">' + htmlMsg + '</div>';
    $("body").append(html);
    $(".tel-dialog").dialog(dialogOptions);
}
function displayConfirmationMsg(htmlMsg, title, buttons) {
    var tempOptions = $.extend(true, {}, dialogOptions);
    tempOptions.dialogClass = "animated wobble";
    tempOptions.buttons = buttons;
    var html = '<div class="tel-confirm-dialog" title="<i class=fe-flash></i>' + title + '">' + htmlMsg + '</div>';
    $("body").append(html);
    $(".tel-confirm-dialog").dialog(tempOptions);
}
function applyDateTemplate(date, classes) {
    return '<li data-date="' + date + '" ' + (classes != null ? ' class="' + classes + '">' : '>') +
           '<span>' + Date.parse(date).toString("MMMM dd, yyyy") + '</span>' +
           '<div class="wo-actions">' +
           '<a href="#" class="del-action" title="Delete"><i class="fe-trash"></i></a><a href="#"  class="edit-action" title="Edit"><i class="fe-pencil-2"></i></a>' +
           '</div></li>';
}
function applyLocationTemplate(loc, classes, comment) {
    return '<li class="clearfix ' + (classes != null ? classes : '') + '">' +
           '<span>' + loc + '</span><span style="display:none;">' + comment + '</span>' +
                '<div class="wo-actions">' +
                    '<a href="#" class="del-action" title="Delete"><i class="fe-trash"></i></a>' +
                    '<a href="#" class="edit-action" title="Edit"><i class="fe-pencil-2"></i></a>' +
                '</div></li>';
}
function applyActivityTemplate(acts) {
    var actBox = '<li class="clearfix new-act"><div class="act-holder"><select class="act-ddl" name="acts"><option></option>';
    for (x = 0; x < acts.length; x++) {
        var act = acts[x];
        actBox += '<option value="' + act.activityID + '">' + act.description + '</option>';
    }
    actBox += '</select><input type="text" class="act-amt" /></div>' +
        '<div class="wo-actions"><a href="#" class="del-action" title="Delete"><i class="fe-trash"></i></a></div></li>';
    return actBox;
}
function applyActivityMatTemplate(acts) {
    var actBox = '<li class="clearfix new-act"><div class="act-holder"><select class="act-ddl" name="acts"><option></option>';
    for (x = 0; x < acts.length; x++) {
        var act = acts[x];
        actBox += '<option value="' + act.activityID + '">' + act.description + '</option>';
    }
    actBox += '</select><input type="text" class="act-amt"/></div>';
    var matReq = $("#matsReqHidden").val();
    actBox += (matReq == "true" ? '' : '<div class="wo-actions"><a href="#" class="del-action" title="Delete"><i class="fe-trash"></i></a></div></li>');
    return actBox;
}

function applyQuotActivityTemplate(acts) {
    var actBox = '<li class="clearfix new-act"><div class="act-holder"><select class="act-ddl" name="acts"><option></option>';
    for (x = 0; x < acts.length; x++) {
        var act = acts[x];
        actBox += '<option value="' + act.activityID + '">' + act.description + '</option>';
    }
    actBox += '</select><input type="text" class="act-rat" style="width: 70px;"/><input type="text" class="act-amt" /></div>' +
        '<div class="wo-actions"><a href="#" class="del-action" title="Delete"><i class="fe-trash"></i></a></div></li>';
    return actBox;
}
function applySInvActivityTemplate(acts) {
    var actBox = '<li class="clearfix new-act"><div class="act-holder"><select class="act-ddl" name="acts"><option></option>';
    for (x = 0; x < acts.length; x++) {
        var act = acts[x];
        actBox += '<option value="' + act.activityID + '">' + act.description + '</option>';
    }
    actBox += '</select><input type="text" class="act-rat" style="width: 70px;"/><input type="text" class="act-amt" /></div>' +
        '<div class="wo-actions"><a href="#" class="del-action" title="Delete"><i class="fe-trash"></i></a></div></li>';
    return actBox;
}

function applyActivityTemplateWithValues(acts, actId, actAmt) {
    
    var actBox = '<li class="clearfix new-act"><div class="act-holder"><select class="act-ddl" name="acts"><option></option>';
    for (x = 0; x < acts.length; x++) {
        var act = acts[x];
        actBox += '<option value="' + act.activityID + '" ' + (act.activityID == actId ? 'selected="selected"' : '') + '>' + act.description + '</option>';
    }
    actBox += '</select><input type="text" class="act-amt" value="' + actAmt + '" /></div>' +
        '<div class="wo-actions"><a href="#" class="del-action"><i class="fe-trash" title="Delete"></i></a></div></li>';
    return actBox;
}
function discardDataCheck(type) {
    if (wochanged)
        return true;
    switch (type) {
        case "full":
            return $("#constructor .const-holder ul li[class*='new-']").length > 0;
            break;
        case "acts":
            return $("#constructor .const-locs ul li.new-loc, #constructor .const-acts ul li.new-act").length > 0;
            break;
        case "locs":
            return $("#constructor .const-acts ul li.new-act").length > 0;
            break;
    }
}
function generateLocations() {
    $("#constructor .const-holder.const-dates ul li").removeClass("active");
    $(".const-main .const-acts .placeholder").show(200).find("button").removeClass("activated");
    $(".const-main .const-acts ul").remove();
    var self = temp;
    self.addClass("active");
    var locHolder = $(".const-main .const-holder.const-locs");
    if (self.hasClass("new-date")) {
        var data = {
            wid: $("#constructor").data("id"),
            invID: $("#constructor").data("inv"),
            date: self.data("date"),
            type: $("#constructor").hasClass("view-wo") ? "view" : ""
        };
        locHolder.find("ul").remove();
        $(".const-main .const-locs .placeholder").show(100, function () {
            $(".const-main .const-locs .placeholder button").addClass("activated animated flash");
        });
        if (!AddLocationsFromArry(data)) {

        }
    }
    else {
        var data = {
            wid: $("#constructor").data("id"),
            invID: $("#constructor").data("inv"),
            date: self.data("date"),
            type: $("#constructor").hasClass("view-wo") ? "view" : ""
        };
        $.post("/workorder/getconstructorlocations", data, function (html) {
            var ph = locHolder.find(".placeholder");
            var time = ph.is(":visible") ? 300 : 10;
            ph.find("button").removeClass("activated animated flash")
            ph.hide(200);
            setTimeout(function () {
                locHolder.find("ul").remove();
                locHolder.append(html);
                AddLocationsFromArry(data);
                $('.const-main .const-locs ul').perfectScrollbar();
            }, time);
        });
    }
}
function generateActivities() {
    $("#constructor .const-holder.const-locs ul li").removeClass("active");
    var self = temp;
    self.addClass("active");
    var actHolder = $(".const-main .const-holder.const-acts");
    if (self.hasClass("new-loc")) {
        var data = {
            wid: $("#constructor").data("id"),
            invID: $("#constructor").data("inv"),
            date: $("#constructor .const-holder.const-dates ul li.active").data("date"),
            location: $(self.find("span")[0]).text(),
            type: $("#constructor").hasClass("view-wo") ? "view" : ""
        };
        actHolder.find("ul").remove();
        $(".const-main .const-acts .placeholder").show(100, function () {
            $(".const-main .const-acts .placeholder button").addClass("activated animated flash");
        });
        if (!AddActivitiesFromArry(data)) {
            //actHolder.find("ul").remove();
            
        }
    }
    else {
        var data = {
            wid: $("#constructor").data("id"),
            invID: $("#constructor").data("inv"),
            date: $("#constructor .const-holder.const-dates ul li.active").data("date"),
            location: self.find("span.loc-details").text(),
            type: $("#constructor").hasClass("view-wo") ? "view" : ""
        };
        if (data.location == "")
            data.location = $(self.find("span")[0]).text();
        $.post("/workorder/getconstructoractivities", data, function (html) {
            var ph = actHolder.find(".placeholder");
            var time = ph.is(":visible") ? 300 : 10;
            ph.find("button").removeClass("activated animated flash");
            ph.hide(200);
            setTimeout(function () {
                
                actHolder.find("ul").remove();
                actHolder.append(html);
                AddActivitiesFromArry(data);
                setDisableActivityOptions();
                actHolder.find("select").select2({ width: "resolve" });
                actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                $('.const-main .const-acts ul').perfectScrollbar();
            }, time);
        });
    }
}
function AddActivitiesFromArry(data) {    
    var actAvailable = false;
    var actsHolder = $("#constructor > .const-main .const-acts");
    if (CheckIsActExist(data.location, data.date)) {
        
        if (actsHolder.find("ul").length == 0)
            actsHolder.append("<ul></ul>");
        var ph = $("#constructor .const-acts .placeholder");
        var time = ph.is(":visible") ? 300 : 10;
        ph.hide(200);
    }
    for (var i = 0; i < tempActs.length; i++) {
        if (tempActs[i].location == data.location && tempActs[i].actDate == data.date && !CheckIsInPlaceHolder(tempActs[i].actID)) {
            actsHolder.find("ul").append(applyActivityTemplateWithValues(actsTempStorage, tempActs[i].actID, tempActs[i].actQty));
            $('.const-main .const-acts ul').perfectScrollbar("update");
            setDisableActivityOptions();
            actsHolder.find("select").select2({ width: "resolve", placeholder: "Select an Activity" });
            actsHolder.find("input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
            actAvailable = true;
        }
    }
    return actAvailable;
}
function AddLocationsFromArry(data) {
    var locAvailable = false;
    var locsHolder = $("#constructor > .const-main .const-locs");
    if (CheckIsLocExist(data.location, data.date)) {
        if (locsHolder.find("ul").length == 0)
            locsHolder.append("<ul></ul>");
        var ph = $("#constructor .const-locs .placeholder");
        var time = ph.is(":visible") ? 300 : 10;
        ph.hide(200);
    }
    for (var i = 0; i < tempLocs.length; i++) {
        if (tempLocs[i].actDate == data.date && !CheckIsLocInPlaceHolder(tempLocs[i].location)) {
            locsHolder.find("ul").append(applyLocationTemplate(tempLocs[i].location, "new-loc", tempLocs[i].comment));
            $('.const-main .const-locs ul').perfectScrollbar("update");
            locAvailable = true;
        }
    }
    return locAvailable;
}
function CheckIsActExist(loc, date) {
    var isExist = false;
    for (var i = 0; i < tempActs.length; i++) {
        if (tempActs[i].location == loc && tempActs[i].actDate == date) {
            isExist = true;
            break;
        }
    }
    return isExist;
}
function CheckIsLocExist(date) {
    var isExist = false;
    for (var i = 0; i < tempLocs.length; i++) {
        if (tempLocs[i].actDate == date) {
            isExist = true;
            break;
        }
    }
    return isExist;
}
function CheckIsInPlaceHolder(actId) {
    var inPlace = false;
    //var loc = $($(this).find("span")[0]).text();
    //var actDate = $(".const-main .const-holder.const-dates ul li.active").data("date");
    $(".const-main .const-holder.const-acts ul li").each(function () {
        var self = $(this);
        var actID = self.find("select").val();
        if (actId == actID) {
            inPlace = true;
        }
    });
    return inPlace;
}
function CheckIsLocInPlaceHolder(loc) {
    var inPlace = false;
    //var loc = $($(this).find("span")[0]).text();
    //var actDate = $(".const-main .const-holder.const-dates ul li.active").data("date");
    $(".const-main .const-holder.const-locs ul li").each(function () {
        var self = $(this);
        var lc = $(self.find("span")[0]).text();
        if (loc == lc) {
            inPlace = true;
        }
    });
    return inPlace;
}
function setupLocationDialogBox(val, adtlDetail) {
    var locsHolder = $("#constructor > .const-main .const-locs");
    var tempOptions = $.extend(true, {}, dialogOptions);
    tempOptions.dialogClass = "animated rubberBand";
    tempOptions.buttons = [{
        text: "Save", click: function () {
            var locElm = $(this).find("textarea.text-loc");
            var loc = locElm.val();
            var commentElm = $(this).find("textarea.text-comment");
            var commentdetil = commentElm.val();
            if (loc.isEmpty()) {
                locElm.css("border-color", "red");
            }
            if (commentdetil.isEmpty()) {
                commentElm.css("border-color", "red");
            }
            else {
                var ph = $("#constructor .const-locs .placeholder");
                var time = ph.is(":visible") ? 300 : 10;
                ph.hide(200);
                var loc = $(this).find("textarea.text-loc").val();
                var comment = $(this).find("textarea.text-comment").val();
                var wid = $("#constructor").data("id");
                var date = $("#constructor .const-dates li.active").data("date");
                var oldLoc = temp.find("span").text();
                var newLoc = loc;
                setTimeout(function () {
                    if (locElm.hasClass("editing")) {
                        $.post("/workorder/UpdateWorkOrderLocation", { wid: wid, date: date, newLocation: newLoc, oldLocation: val, aditinalDetail: comment }, function () {
                            temp.replaceWith(applyLocationTemplate(loc, temp.attr("class"), comment));
                        });
                    }
                    else {
                        if (locsHolder.find("ul").length == 0)
                            locsHolder.append("<ul></ul>");

                        $.post("/workorder/UpdateWorkOrderLocation", { wid: wid, date: date, newLocation: newLoc, oldLocation: "newrec", aditinalDetail: comment }, function () {
                            //temp.replaceWith(applyLocationTemplate(loc, temp.attr("class"), comment));
                        });
                        locsHolder.find("ul").append(applyLocationTemplate(loc, "new-loc", comment));
                    }
                    $('.const-main .const-locs ul').perfectScrollbar("update");
                }, time);
                $(this).dialog("close");
            }
        }
    }];
    $("body").append('<div class="const-loc-modal" title="<i class=fe-location-2></i> Add Location">' +
        '<lable style="float: left;padding-bottom: 5px;padding-left: 10px;">Location</lable><br /><textarea class="text-loc' + (val != "" ? ' editing"' : '"') + '>' + val + '</textarea>' +
        '<lable style="float: left;padding-bottom: 5px;padding-left: 10px;">Comment</lable><br /><textarea class="text-comment' + (adtlDetail != "" ? ' editing"' : '"') + '>' + adtlDetail + '</textarea></div>');
    $(".const-loc-modal").dialog(tempOptions);
}
function setupActivityCalendar(date) {
    var dates = [];
    var datesHolder = $("#constructor > .const-main .const-dates");
    datesHolder.find("ul li").each(function () {
        var dt = $(this).data("date");
        dates.push(Date.parse(dt).toString("M/dd/yyyy"));
    });
    var tempOptions = $.extend(true, {}, dialogOptions);
    tempOptions.dialogClass = "animated rubberBand";
    tempOptions.buttons = [{
        text: "Save", click: function () {
            var dateObj = $(".const-date-modal .cal");
            var dt = dateObj.datepicker('getDate');
            var dtIndex = dt.toString().indexOf(" (");
            var parsedDt = dt.toString().substr(0, dtIndex);
            var newDate = Date.parse(parsedDt).toString("M/dd/yyyy HH:mm");
            var ph = $("#constructor .const-dates .placeholder");
            var time = ph.is(":visible") ? 300 : 10;
            ph.hide(200);
            var wid = $("#constructor").data("id");
            var newDate = newDate;

            setTimeout(function () {
                if (dateObj.hasClass("editing")) {
                    var oldDate = temp.data("date");
                    $.post("/workorder/UpdateWorkOrderDate", { wid: wid, newDate: newDate, oldDate: oldDate }, function () {
                        temp.replaceWith(applyDateTemplate(newDate, temp.attr("class")));
                    });
                }
                else {
                    if (datesHolder.find("ul").length == 0)
                        datesHolder.append("<ul></ul>");
                    datesHolder.find("ul").append(applyDateTemplate(newDate, "new-date"));
                }
                $('.const-main .const-dates ul').perfectScrollbar("update");
            }, time);
            $(this).dialog("close");
        }
    }];
    tempOptions.width = 318;
    $("body").append('<div class="const-date-modal" title="<i class=fe-calendar></i> Add Date">' +
        '<div class="cal' + (date != null ? ' editing"' : '"') + '></div></div>');
    var nextAvailableDay = Date.today().toString("M/dd/yyyy");
    var dindex = $.inArray(nextAvailableDay, dates);
    while (dindex > -1) {
        nextAvailableDay = Date.parse(nextAvailableDay).addDays(1).toString("M/dd/yyyy");
        dindex = $.inArray(nextAvailableDay, dates);
    }
    if (date == null)
        date = nextAvailableDay;
    else {
        delete dates[$.inArray(Date.parse(date).toString("M/dd/yyyy"), dates)];
        date = Date.parse(date).toString("M-d-yyyy");
    }
    $(".const-date-modal .cal").datepicker({
        todayHighlight: true,
        format: "D M dd, yyyy",
        beforeShowDay: function (date) {
            var d = Date.parse(date.toDateString()).toString("M/dd/yyyy");
            return $.inArray(d, dates) == -1;
        }
    }).datepicker('setDate', date);
    $(".const-date-modal").dialog(tempOptions);
}
function setDisableActivityOptions() {
    actsSelectedValue = [];
    var selects = $("#constructor .const-acts .act-holder select.act-ddl");
    selects.find("option.elsewhere").removeAttr("disabled");
    selects.each(function () {
        var val = $(this).val();
        actsSelectedValue.push(val);
    });
    for (var x = 0; x < actsSelectedValue.length; x++) {
        var val = actsSelectedValue[x];
        selects.each(function () {
            var self = $(this);
            var option = self.find("option[value='" + val + "']");
            if (self.val() != val) {
                option.addClass("elsewhere").attr("disabled", "disabled");
            }
            else {
                option.removeClass("elsewhere")
            }
        });
    }
}
function deleteAction(type) {
    var val = type == "Date" ? temp.data("date") : type == "Location" ? temp.find("span.loc-details").text() : temp.data("id");
    var invid = $("#constructor").data("inv");
    var wid = $("#constructor").data("id");
    $.post("/workorder/deletefromworkorder", { val: val, type: type, wid: wid, invid: invid }, function (data) {
        if (data.Code == "100") {
            temp.remove();
            if (type == "Activity") {
                var actHolder = $(".const-main .const-holder.const-acts");
                if (actHolder.find("li").length < 1) {
                    $(".const-main .const-holder.const-locs li.active").addClass("new-loc");
                    actHolder.find("ul").remove();
                    actHolder.find(".placeholder").show(100, function () {
                        $(this).find("button").addClass("activated animated flash");
                    });
                }
            }
            else if (type == "Location") {
                var locHolder = $(".const-main .const-holder.const-locs");
                var actHolder = $(".const-main .const-holder.const-acts");
                actHolder.find("ul").remove();
                actHolder.find(".placeholder").show(100);
                if (locHolder.find("li").length < 1) {
                    $(".const-main .const-holder.const-dates li.active").addClass("new-date");
                    locHolder.find("ul").remove();
                    locHolder.show(100, function () {
                        locHolder.find("button").addClass("activated animated flash");
                    });
                }
            }
            else {
                var holders = $(".const-main .const-holder.const-locs, .const-main .const-holder.const-acts");
                holders.find("ul").remove();
                holders.find(".placeholder").show(100);
            }
        }
        else {
            var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
            var title = "<i class='fe-comment'></i> Something went wrong";
            displayDialogMsg(msg, title);
        }
        $('.const-main .const-holder ul').perfectScrollbar("update");
    });
}
function updateAction() {
    var actDate = $(".const-main .const-holder.const-dates ul li.active").data("date");
    var actLoc = $($(".const-main .const-holder.const-locs ul li.active").find("span")[0]).text();
    var actLocComment = $($(".const-main .const-holder.const-locs ul li.active").find("span")[1]).text();
    var wID = $("#constructor").data("id");
    var invID = $("#constructor").data("inv");
    var acts = [];
    $(".const-main .const-holder.const-acts ul li").each(function () {
        var self = $(this);
        var actID = self.find("select").val();
        var actQty = self.find(".act-amt").val();
        var matsReq = self.find(".mats-req") != undefined ? self.find(".mats-req").val() : false;
        var mats = [];
        for (var m = 0; m < actMaterialQtyArry.length; m++) {
            if (new Date(actMaterialQtyArry[m].date).toLocaleDateString() == new Date(actDate).toLocaleDateString() && actMaterialQtyArry[m].loc == actLoc && actMaterialQtyArry[m].actID == actID) {
                var matRID = actMaterialQtyArry[m].RID;
                var matId = actMaterialQtyArry[m].matID;
                var matQty = actMaterialQtyArry[m].matQty;
                mats.push({
                    RID: matRID,
                    MetarialId: matId,
                    Amount: matQty
                });
            }
        }
        if (matsReq == "true" && mats.length <= 0)
            return;
        acts.push({
            RecID: self.data("id"),
            RID: actID,
            Amount: actQty,
            HasMetarials: mats.length > 0 ? true : false,
            Metarials: mats
        });
    });
    var data = [];
    data.push({
        WID: wID,
        InvId: invID,
        Date: actDate,
        Location: actLoc,
        AdtnlDetails: actLocComment,
        Activities: acts        
    });
    $(".const-main .const-holder.const-locs ul li").each(function () {
        var loc = $($(this).find("span")[0]).text();
        var locComment = $($(this).find("span")[1]).text();
        if (loc != actLoc) {
            var actsOthr = [];
            for (var i = 0; i < tempActs.length; i++) {
                if (tempActs[i].location == loc && tempActs[i].actDate == actDate) {
                    var id = tempActs[i].id;
                    var actID = tempActs[i].actID;
                    var actQty = tempActs[i].actQty;
                    var actMatsReq = tempActs[i].matsReq;
                    var mats = [];
                    for (var m = 0; m < actMaterialQtyArry.length; m++) {
                        if (actMaterialQtyArry[m].date == actDate && actMaterialQtyArry[m].loc == loc && actMaterialQtyArry[m].actID == actID) {
                            var matRID = actMaterialQtyArry[m].RID;
                            var matId = actMaterialQtyArry[m].matID;
                            var matQty = actMaterialQtyArry[m].matQty;
                            mats.push({
                                RID: matRID,
                                MetarialId: matId,
                                Amount: matQty
                            });
                        }
                    }
                    if (actMatsReq == "true" && mats.length <= 0)
                        return;
                    actsOthr.push({
                        RecID: id,
                        RID: actID,
                        Amount: actQty,
                        HasMetarials: mats.length > 0 ? true : false,
                        Metarials : mats
                    });
                }
            }
            if (actsOthr.length > 0)
                data.push({
                    WID: wID,
                    InvId: invID,
                    Date: actDate,
                    Location: loc,
                    AdtnlDetails: locComment,
                    Activities: actsOthr
                });
        }
    });
    $.post("/workorder/updateactivities", $.toDictionary(data), function (data) {
        if (data.Code == "100") {
            $("#constructor").find("li").removeClass("new-date new-loc");
            temp = $("#constructor .const-holder.const-locs ul li.active");
            generateActivities();
            var msg = "<p>Activities were <b>Updated</b> Successfully!.</p>";
            var title = "<i class='fe-comment'></i> Success";
            actMaterialQtyArry = [];
            tempActs = [];
            //closeConstructor();
            displayDialogMsg(msg, title);
        }
        else {
            var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
            var title = "<i class='fe-comment'></i> Something went wrong";
            displayDialogMsg(msg, title);
        }
    });
    
}
function setupMsgBar(type, msg) {
    var mb = $(".tel-form .msg-bar");
    switch (type) {
        case 1:
            mb.find("i").removeClass("fe-cancel fe-ok").addClass("fe-info-1");
            mb.removeClass("success error").find("span").text(msg);
            mb.addClass("animated pulse");
            break;
        case 2:
            mb.find("i").removeClass("fe-cancel fe-info-1").addClass("fe-ok");
            mb.removeClass("error").addClass("success").find("span").text(msg);
            mb.addClass("animated pulse");
            break;
        default:
            mb.find("i").removeClass("fe-ok fe-info-1").addClass("fe-cancel");
            mb.removeClass("success").addClass("error").find("span").text(msg);
            mb.addClass("animated wobble");
            break;
    }
}
function imageExists(image_url) {
    var http = new XMLHttpRequest();
    http.open('HEAD', image_url, false);
    http.send();
    return http.status != 404;
}
function closeConstructor() {
    $("#constructor").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
        $(this).remove();  
        $(".full-overlay").fadeOut(200, function () { $(this).remove(); });
        if (!$("#con-wo").is(":visible") || $(".tech-tbl").length > 0) {
            var page = dTable.page();
            dTable.page(page).draw(false);
        }
        else {
            var page = aDtable.page();
            aDtable.page(page).draw(false);
            $.get("contractor/refreshConStats", { id: $("#sub-head").data("id") }, function (html) {
                $("#sub-head").replaceWith(html);
            });
        }
    });
}
function loading(above) {
    if (!$(".for-loader").is(":visible")) {
        $("body").append('<div class="full-overlay for-loader' + (above ? ' above' : '') + '"></div>');
        $(".for-loader").fadeIn(100);
        $("body").append('<div class="loading-holder spin"></div><span class="loading-t">T</span>');
    }
}
function removeLoader(hidelb, time, callback) {
    first = time <= 1000 ? 1000 : time - 1000;
    next = time <= 1000 ? time * 2 : time;
    setTimeout(function () {
        $(".loading-holder, .loading-t").fadeOut(200, function () { $(this).remove() });
        $(".for-loader" + (hidelb ? ".above" : "")).fadeOut(200, function () { $(this).remove() });
    }, first);
    setTimeout(function () {
        if (callback && (typeof callback == "function")) {
            callback();
        }
    }, next)
}
function setupTechPrintOuts(options) {
    techTable = $("table.tech-tbl").DataTable(options);
    $(".sub-top.tech-prt").append('<div id="tech-range" class="input-daterange input-group clearfix" id="datepicker">' +
        '<input type="text" class="input-sm form-control" name="start" readonly />' +
        '<span class="input-group-addon">to</span>' +
        '<input type="text" class="input-sm form-control" name="end" readonly />' +
        '<button class="refresh"><i class="fe-cw-outline"></i></button>' +
        '<button class="cancel"><i class="fe-cancel"></i></button></div>');
    $(".dataTables_wrapper select").select2({ minimumResultsForSearch: -1, width: "resolve" });
    $('.sub-top.tech-prt .input-daterange').datepicker({
        format: "DD M dd, yyyy"
    });
}
function setupTeamActions(all) {
    $('.team-setup').find(".number").onlyNumbers({ neagtive: false, decimal: true });
    var selects = all ? $('.team-setup tr .con-ddl') : $('.team-setup tr').last().find('.con-ddl');
    selects.select2({
        placeholder: 'select a contractor',
        width: 260,
        ajax: {
            url: "/workorder/getcontractors",
            dataType: 'json',
            delay: 100,
            data: function (params) {
                var names = [];
                $('.team-setup .con-ddl').each(function () {
                    names.push($(this).val());
                });
                return $.toDictionary({
                    ids: names,
                    query: params.term
                });
            },
            processResults: function (data, page) {
                return { results: data.suggestions }
            }
        },
        escapeMarkup: function (markup) { return markup; },
        templateResult: function (exercise) {
            return "<div class='select2-user-result'>" + exercise.term + "</div>";
        },
        templateSelection: function (exercise) {
            return exercise.term;
        },
        formatNoMatches: function (term) {
            return "<b>No Matches Found</b>"
        },
        initSelection: function (element, callback) {
            var elementText = $(element).attr('data-init-text');
            callback({ "term": elementText });
        }
    });
}
function deleteRow(elm) {
    var url = deleteUrl == null ? elm.parents("table").data("url") + "/delete" : deleteUrl;
    loading(true);
    $.post(url, { id: elm.parents("tr").attr("id") }, function (data) {
        removeLoader(true, 300, function () {
            if (data.Additional > 0) {
                $(".tel-confirm-dialog").dialog("close");
                var page = dTable.page();
                dTable.page(page).draw(false);
                //dTable.draw();
            }
            else {
                $(".tel-confirm-dialog").dialog("close");
                var htmlMsg = "<p>This record cannot be <b class='default-col'>Deleted</b> because it is still <b>Linked</b> to other records in the Database.</p>";
                var htmlTitle = "<i class='fe-comment'></i>Cannot Delete";
                displayDialogMsg(htmlMsg, htmlTitle);
            }
        });
    });
}

/*Meterial Constructor */
function discardMetarialDataCheck(type) {
    if (wochanged)
        return true;
    switch (type) {
        case "full":
            return $("#matConstructor .const-holder ul li[class*='new-']").length > 0;
            break;
        case "acts":
            return $("#matConstructor .const-locs ul li.new-loc, #matConstructor .const-mats ul li.new-act").length > 0;
            break;
        case "locs":
            return $("#matConstructor .const-mats ul li.new-act").length > 0;
            break;
    }
}
function setDisableMetarialOptions() {
    actsSelectedValue = [];
    var selects = $("#matConstructor .const-mats .act-holder select.act-ddl");
    selects.find("option.elsewhere").removeAttr("disabled");
    selects.each(function () {
        var val = $(this).val();
        actsSelectedValue.push(val);
    });
    for (var x = 0; x < actsSelectedValue.length; x++) {
        var val = actsSelectedValue[x];
        selects.each(function () {
            var self = $(this);
            var option = self.find("option[value='" + val + "']");
            if (self.val() != val) {
                option.addClass("elsewhere").attr("disabled", "disabled");
            }
            else {
                option.removeClass("elsewhere")
            }
        });
    }
}
function deleteMetarial(type) {
    var val = temp.data("id");
    var invid = $("#matConstructor").data("inv");
    var wid = $("#matConstructor").data("id");
    if (val != undefined && val != 0 && val != -1) {
        $.post("/workorder/deletemetarialfromworkorder", { val: val, type: type, wid: wid, invid: invid }, function (data) {
            if (data.Code == "100") {
                temp.remove();
                if (type == "Activity") {
                    var actHolder = $(".mat-const-main .const-holder.const-mats");
                    if (actHolder.find("li").length < 1) {
                        actHolder.find("ul").remove();
                        actHolder.find(".placeholder").show(100, function () {
                            $(this).find("button").addClass("activated animated flash");
                        });
                    }
                }
            }
            else {
                var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
                var title = "<i class='fe-comment'></i> Something went wrong";
                displayDialogMsg(msg, title);
            }
            $('.mat-const-main .const-holder ul').perfectScrollbar("update");
        });
    }
    else {
        var index = -1;
        for (var i = 0; i < actMaterialQtyArry.length; i++) {
            if (actMaterialQtyArry[i].actID == actID && actMaterialQtyArry[i].loc == loc && actMaterialQtyArry[i].date == date && actMaterialQtyArry[i].matID == matID) {
                index = i;
                break
            }
        }
        if(index != -1)
            fruits.splice(index, 1);
    }
}
function updateMetarial() {
    $(".mat-const-main .const-holder.const-mats ul li").each(function () {
        var self = $(this);
        var actID = $("#matConstructor #actIdHidden").val();
        var loc = $("#matConstructor #locHidden").val();
        var date = $("#matConstructor #dateHidden").val();        
        var matID = self.find("select").val();
        var matQty = self.find(".act-amt").val();
        if (!matID.isEmpty() && !matQty.isEmpty()) {
            var isExist = false;
            for (var i = 0; i < actMaterialQtyArry.length; i++) {
                if (actMaterialQtyArry[i].actID == actID && actMaterialQtyArry[i].loc == loc && actMaterialQtyArry[i].date == date && actMaterialQtyArry[i].matID == matID) {
                    actMaterialQtyArry[i].matQty = matQty;
                    isExist = true;                    
                }
            }
            if (!isExist) {
                actMaterialQtyArry.push({
                    RID: self.data("id"),
                    actID: actID,
                    loc: loc,
                    date: date,
                    matID: matID,
                    matQty: matQty
                });
            }
        }
    });
    isMatAddedToist = true;
    //var wID = $("#matConstructor").data("id");
    //var invID = $("#matConstructor").data("inv");
    //var mats = [];
    //$(".mat-const-main .const-holder.const-mats ul li").each(function () {
    //    var self = $(this);
    //    var actID = self.find("select").val();
    //    var actQty = self.find(".act-amt").val();
    //    mats.push({
    //        RecID: self.data("id"),
    //        RID: actID,
    //        Amount: actQty
    //    });
    //});
    //var data = [];
    //data.push({
    //    WID: wID,
    //    InvId: invID,
    //    Activities: mats
    //});
    //$.post("/workorder/updateMetarials", $.toDictionary(data), function (data) {
    //    if (data.Code == "100") {
    //        ReLoadMetarias(wID);
    //        var msg = "<p>Metarials were <b>Updated</b> Successfully!.</p>";
    //        var title = "<i class='fe-comment'></i> Success";
    //        //displayDialogMsg(msg, title);
    //    }
    //    else {
    //        var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
    //        var title = "<i class='fe-comment'></i> Something went wrong";
    //        displayDialogMsg(msg, title);
    //    }
    //});
    closeMatConstructor();
}
function ReLoadMetarias(wid) {
    $.ajax({
        url: "/workorder/GenerateMetarialConstructor",
        data: { wid: wid },
        type: "POST",
        success: function (html) {
            removeLoader(false, 500, function () {
                $("body").append(html);
                $("#matConstructor [title]").tooltip({ placement: "top" });
                $("#matConstructor").addClass("animated bounceInRight").show();
                $('.mat-const-main .const-holder ul').perfectScrollbar();
                $("#matConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                $("#matConstructor .title span").dotdotdot();
                var actHolder = $(".mat-const-main .const-holder.const-mats");
                if (actHolder.find("li").length == 0) {
                    actHolder.find("ul").remove();
                    $(".mat-const-main .const-mats .placeholder").show(100, function () {
                        $(".mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
                    });
                }
                else {
                    var ph = actHolder.find(".placeholder");
                    var time = ph.is(":visible") ? 300 : 10;
                    ph.find("button").removeClass("activated animated flash");
                    ph.hide(200);
                    setTimeout(function () {
                        setDisableMetarialOptions();
                        actHolder.find("select").select2({ width: "resolve" });
                        actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                        $('.mat-const-main .const-mats ul').perfectScrollbar();
                    }, time);
                }
            });
        }
    });
}
function closeMatConstructor() {
    $("#matConstructor").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
        $(this).remove();
        $(".full-overlay").fadeOut(200, function () { $(this).remove(); });
        if (!$("#con-wo").is(":visible") || $(".tech-tbl").length > 0) {
            //var page = dTable.page();
            //dTable.page(page).draw(false);
        }
        else {
            //var page = aDtable.page();
            //aDtable.page(page).draw(false);
            $.get("contractor/refreshConStats", { id: $("#sub-head").data("id") }, function (html) {
                $("#sub-head").replaceWith(html);
            });
        }
    });
}
/*End Meterial Constructor*/

/*Quotation Constructor */
function discardQuotationDataCheck(type) {
    if (wochanged)
        return true;
    switch (type) {
        case "full":
            return $("#qConstructor .const-holder ul li[class*='new-']").length > 0;
            break;
        case "acts":
            return $("#qConstructor .const-locs ul li.new-loc, #qConstructor .const-mats ul li.new-act").length > 0;
            break;
        case "locs":
            return $("#qConstructor .const-mats ul li.new-act").length > 0;
            break;
    }
}
function setDisableQuotationActivityOptions() {
    actsSelectedValue = [];
    var selects = $("#qConstructor .const-mats .act-holder select.act-ddl");
    selects.find("option.elsewhere").removeAttr("disabled");
    selects.each(function () {
        var val = $(this).val();
        actsSelectedValue.push(val);
    });
    for (var x = 0; x < actsSelectedValue.length; x++) {
        var val = actsSelectedValue[x];
        selects.each(function () {
            var self = $(this);
            var option = self.find("option[value='" + val + "']");
            if (self.val() != val) {
                option.addClass("elsewhere").attr("disabled", "disabled");
            }
            else {
                option.removeClass("elsewhere")
            }
        });
    }
}
function deleteQuotationActivity(type) {
    var val = temp.data("id");
    $.post("/quotation/deleteactivityfromquotation", { val: val }, function (data) {
        if (data.Code == "100") {
            temp.remove();
            if (type == "Activity") {
                var actHolder = $("#qConstructor .mat-const-main .const-holder.const-mats");
                if (actHolder.find("li").length < 1) {
                    actHolder.find("ul").remove();
                    actHolder.find(".placeholder").show(100, function () {
                        $(this).find("button").addClass("activated animated flash");
                    });
                }
            }
        }
        else {
            var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
            var title = "<i class='fe-comment'></i> Something went wrong";
            displayDialogMsg(msg, title);
        }
        $('#qConstructor .mat-const-main .const-holder ul').perfectScrollbar("update");
    });
}
function updateQuotationActivity() {
    var qID = $("#qConstructor").data("id");
    var acts = [];
    $("#qConstructor .mat-const-main .const-holder.const-mats ul li").each(function () {
        var self = $(this);
        var actID = self.find("select").val();
        var actQty = self.find(".act-amt").val();
        var actRate = self.find(".act-rat").val();
        acts.push({
            RecID: self.data("id"),
            ActDescr: actID,
            Qty: actQty,
            Rate: actRate
        });
    });
    var data = [];
    data.push({
        QID: qID,
        Activities: acts
    });
    $.post("/quotation/updateActivities", $.toDictionary(data), function (data) {
        if (data.Code == "100") {
            closeQuotConstructor();
            //ReLoadQuotationActivities(qID);
            var msg = "<p>Activities were <b>Updated</b> Successfully!.</p>";
            var title = "<i class='fe-comment'></i> Success";
            //displayDialogMsg(msg, title);
        }
        else {
            var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
            var title = "<i class='fe-comment'></i> Something went wrong";
            displayDialogMsg(msg, title);
        }
    });    
}
function ReLoadQuotationActivities(qid) {
    $.ajax({
        url: "/quotation/GenerateConstructor",
        data: { qid: qid },
        type: "POST",
        success: function (html) {
            removeLoader(false, 500, function () {
                $("body").append(html);
                $("#qConstructor [title]").tooltip({ placement: "top" });
                $("#qConstructor").addClass("animated bounceInRight").show();
                $('#qConstructor .mat-const-main .const-holder ul').perfectScrollbar();
                $("#qConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                $("#qConstructor .title span").dotdotdot();
                var actHolder = $(".mat-const-main .const-holder.const-mats");
                if (actHolder.find("li").length == 0) {
                    actHolder.find("ul").remove();
                    $("#qConstructor .mat-const-main .const-mats .placeholder").show(100, function () {
                        $("#qConstructor .mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
                    });
                }
                else {
                    var ph = actHolder.find(".placeholder");
                    var time = ph.is(":visible") ? 300 : 10;
                    ph.find("button").removeClass("activated animated flash");
                    ph.hide(200);
                    setTimeout(function () {
                        setDisableQuotationActivityOptions();
                        actHolder.find("select").select2({ width: "resolve" });
                        actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                        $('#qConstructor .mat-const-main .const-mats ul').perfectScrollbar();
                    }, time);
                }
            });
        }
    });
}
function closeQuotConstructor() {
    $("#qConstructor").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
        $(this).remove();
        $(".full-overlay").fadeOut(200, function () { $(this).remove(); });
        if (!$("#con-wo").is(":visible") || $(".tech-tbl").length > 0) {
            var page = dTable.page();
            dTable.page(page).draw(false);
        }
        else {
            var page = aDtable.page();
            aDtable.page(page).draw(false);
            $.get("contractor/refreshConStats", { id: $("#sub-head").data("id") }, function (html) {
                $("#sub-head").replaceWith(html);
            });
        }
    });
}
/*End Quotation Constructor*/


/*Standby Invoice Constructor */
function discardSInvoiceDataCheck(type) {
    if (wochanged)
        return true;
    switch (type) {
        case "full":
            return $("#sIConstructor .const-holder ul li[class*='new-']").length > 0;
            break;
        case "acts":
            return $("#sIConstructor .const-locs ul li.new-loc, #sIConstructor .const-mats ul li.new-act").length > 0;
            break;
        case "locs":
            return $("#sIConstructor .const-mats ul li.new-act").length > 0;
            break;
    }
}
function setDisableSInvoiceActivityOptions() {
    actsSelectedValue = [];
    var selects = $("#sIConstructor .const-mats .act-holder select.act-ddl");
    selects.find("option.elsewhere").removeAttr("disabled");
    selects.each(function () {
        var val = $(this).val();
        actsSelectedValue.push(val);
    });
    for (var x = 0; x < actsSelectedValue.length; x++) {
        var val = actsSelectedValue[x];
        selects.each(function () {
            var self = $(this);
            var option = self.find("option[value='" + val + "']");
            if (self.val() != val) {
                option.addClass("elsewhere").attr("disabled", "disabled");
            }
            else {
                option.removeClass("elsewhere")
            }
        });
    }
}
function deleteSInvoiceActivity(type) {
    var val = temp.data("id");
    $.post("/StandbyInvoice/DeleteActivityFromStandbyInvoice", { val: val }, function (data) {
        if (data.Code == "100") {
            temp.remove();
            if (type == "Activity") {
                var actHolder = $("#sIConstructor .mat-const-main .const-holder.const-mats");
                if (actHolder.find("li").length < 1) {
                    actHolder.find("ul").remove();
                    actHolder.find(".placeholder").show(100, function () {
                        $(this).find("button").addClass("activated animated flash");
                    });
                }
            }
        }
        else {
            var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
            var title = "<i class='fe-comment'></i> Something went wrong";
            displayDialogMsg(msg, title);
        }
        $('#sIConstructor .mat-const-main .const-holder ul').perfectScrollbar("update");
    });
}
function updateSInvoiceActivity() {
    var sIID = $("#sIConstructor").data("id");
    var acts = [];
    $("#sIConstructor .mat-const-main .const-holder.const-mats ul li").each(function () {
        var self = $(this);
        var actID = self.find("select").val();
        var actQty = self.find(".act-amt").val();
        var actRate = self.find(".act-rat").val();
        acts.push({
            RecID: self.data("id"),
            ActId: actID,
            Qty: actQty,
            Rate: actRate
        });
    });
    var data = [];
    data.push({
        SIID: sIID,
        Activities: acts
    });
    $.post("/StandbyInvoice/updateActivities", $.toDictionary(data), function (data) {
        if (data.Code == "100") {
            closeSInvoiceConstructor();
            //ReLoadSInvoiceActivities(sIID);
            var msg = "<p>Activities were <b>Updated</b> Successfully!.</p>";
            var title = "<i class='fe-comment'></i> Success";
            //displayDialogMsg(msg, title);
        }
        else {
            var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
            var title = "<i class='fe-comment'></i> Something went wrong";
            displayDialogMsg(msg, title);
        }
    });
}
function ReLoadSInvoiceActivities(sIid) {
    $.ajax({
        url: "/StandbyInvoice/GenerateConstructor",
        data: { sIid: sIid },
        type: "POST",
        success: function (html) {
            removeLoader(false, 500, function () {
                $("body").append(html);
                $("#sIConstructor [title]").tooltip({ placement: "top" });
                $("#sIConstructor").addClass("animated bounceInRight").show();
                $('#sIConstructor .mat-const-main .const-holder ul').perfectScrollbar();
                $("#sIConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                $("#sIConstructor .title span").dotdotdot();
                var actHolder = $(".mat-const-main .const-holder.const-mats");
                if (actHolder.find("li").length == 0) {
                    actHolder.find("ul").remove();
                    $("#sIConstructor .mat-const-main .const-mats .placeholder").show(100, function () {
                        $("#sIConstructor .mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
                    });
                }
                else {
                    var ph = actHolder.find(".placeholder");
                    var time = ph.is(":visible") ? 300 : 10;
                    ph.find("button").removeClass("activated animated flash");
                    ph.hide(200);
                    setTimeout(function () {
                        setDisableSInvoiceActivityOptions();
                        actHolder.find("select").select2({ width: "resolve" });
                        actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                        $('#sIConstructor .mat-const-main .const-mats ul').perfectScrollbar();
                    }, time);
                }
            });
        }
    });
}
function closeSInvoiceConstructor() {
    $("#sIConstructor").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
        $(this).remove();
        $(".full-overlay").fadeOut(200, function () { $(this).remove(); });
        if (!$("#con-wo").is(":visible") || $(".tech-tbl").length > 0) {
            var page = dTable.page();
            dTable.page(page).draw(false);
        }
        else {
            var page = aDtable.page();
            aDtable.page(page).draw(false);
            $.get("contractor/refreshConStats", { id: $("#sub-head").data("id") }, function (html) {
                $("#sub-head").replaceWith(html);
            });
        }
    });
}
/*End Standby Invoice Constructor*/
