$(document).ready(function () {
	// Add minus icon for collapse element which is open by default
	$(".collapse.in").each(function () {
		$(this).siblings(".panel-heading").find(".glyphicon").addClass("glyphicon-minus").removeClass("glyphicon-plus");
	});

	// Toggle plus minus icon on show hide of collapse element
	$(".collapse").on('show.bs.collapse', function () {
		$(this).parent().find(".glyphicon").removeClass("glyphicon-plus").addClass("glyphicon-minus");
	}).on('hide.bs.collapse', function () {
		$(this).parent().find(".glyphicon").removeClass("glyphicon-minus").addClass("glyphicon-plus");
	});
});

function GetClassRoomsForSchool(schoolID, elementID, defaultText, urlAction) {
	var procemessage = "<option value='0'> Please wait...</option>";
	$("#" + elementID).html(procemessage).show();
	var url = urlAction;

	$.ajax({
		url: url,
		data: { SchoolID: schoolID },
		cache: false,
		type: "POST",
		success: function (data) {
			console.log(data);
			var markup = "<option value>" + defaultText + "</option>";
			for (var x = 0; x < data.length; x++) {
				markup += "<option value=" + data[x].Value;
				if (data.length == 1) {
					markup += " selected";
				}
				markup += ">" + data[x].Text + "</option>";
			}
			$("#" + elementID).html(markup).show();
			$("#" + elementID).prop('disabled', data.length === 0);
		},
		error: function () {
			$("#" + elementID).html("<option value>" + defaultText + "</option>").show();
			$("#" + elementID).prop('disabled', true);
		}
	});
}

function GetTemplates(schoolID, classRoomID, elementID, defaultText, urlAction, callBack) {
	var procemessage = "<option value='0'> Please wait...</option>";
	$("#" + elementID).html(procemessage).show();
	var url = urlAction;

	$.ajax({
		url: url,
		data: { SchoolID: schoolID, ClassRoomID: classRoomID },
		cache: false,
		type: "POST",
		success: function (data) {
			console.log("GetTemplates Returned Success");
			console.log(data);
			var markup = "<option value>" + defaultText + "</option>";
			for (var x = 0; x < data.length; x++) {
				markup += "<option value=" + data[x].Value;
				//if (data.length == 1) {
				//	markup += " selected";
				//}
				markup += ">" + data[x].Text + "</option>";
			}
			$("#" + elementID).html(markup).show();
			console.log("data length == ");
			console.log(data.length);
			$("#" + elementID).prop('disabled', data.length === 0);
			console.log(callBack);
			if (callBack !== null) {
				console.log("Executing CallBack");
				callBack();
			}
			console.log("CallBack Done");
		},
		error: function () {
			console.log("GetTemplates Returned Error");
			$("#" + elementID).html("<option value>" + defaultText + "</option>").show();
			$("#" + elementID).prop('disabled', true);
			if (callBack !== null) {
				callBack();
			}
		}
	});
}

function GetTemplateByID(templateID) {
	var templateJSON = null;
	if (templateID !== null && templateID > 0) {
		$.ajax({
			url: "/Templates/GetTemplateForPermissionSlip/",
			data: { TemplateID: templateID },
			async: false,
			cache: false,
			type: "POST",
			success: function (data) {
				console.log("GetTemplatesByID Success");
				console.log(data);
				templateJSON = data;
			},
			error: function (xhr, ajaxOptions, thrownError) {
				console.log("GetTemplatesByID Error");
				console.log(xhr);
			}
		});
	}

	return templateJSON;
}


