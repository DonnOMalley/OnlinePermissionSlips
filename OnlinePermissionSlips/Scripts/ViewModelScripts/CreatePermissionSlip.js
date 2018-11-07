$(function () {
	$('#StartDateTime').datetimepicker({
		daysOfWeekDisabled: [0, 6],
		sideBySide: true
	});
});

$(function () {
	$('#EndDateTime').datetimepicker({
		daysOfWeekDisabled: [0, 6],
		sideBySide: true
	});
});

function SchoolSelected(schoolID) {
	var originalTemplateID = $('#PermissionSlipTemplateID').val();
	var CallBack = function () { TemplateCallBack(originalTemplateID); };
	GetClassRoomsForSchool(schoolID, 'ClassRoomID', 'Select Class Room', '/ClassRooms/GetSchoolClasses/');
	if (!$('#chkTemplateBySchool').prop('checked')) {
		GetTemplates(schoolID, null, 'PermissionSlipTemplateID', 'No Template', '/Templates/GetTemplateList/', CallBack);
	}
}

function ClassSelected(classID) {
	var originalTemplateID = $('#PermissionSlipTemplateID').val();
	var CallBack = function () { TemplateCallBack(originalTemplateID); };
	if ($('#chkTemplateBySchool').prop('checked')) {
		var schoolID = $('#SchoolID').val();
		GetTemplates(schoolID, classID, 'PermissionSlipTemplateID', 'No Template', '/Templates/GetTemplateList/', CallBack);
	}
}

function LimitChanged() {
	var classID = null;
	var schoolID = $('#SchoolID').val();
	var originalTemplateID = $('#PermissionSlipTemplateID').val();
	var CallBack = function () { TemplateCallBack(originalTemplateID); };

	if ($('#chkTemplateBySchool').prop('checked')) {
		classID = $('#ClassRoomID').val();
		if (classID == "") { classID = -1; }
	}
	GetTemplates(schoolID, classID, 'PermissionSlipTemplateID', 'No Template', '/Templates/GetTemplateList/', CallBack);
}

function TemplateCallBack(originalTemplateID) {
	var TemplateID = $('#PermissionSlipTemplateID').val();
	if (TemplateID == "") {
		TemplateID = originalTemplateID;
	}
	if (TemplateID == "") { console.log("Empty Template"); } else { console.log("Template ID"); }

	//How to handle the Template ID list being cleared/Different based on changed list
	var exists = false;
	$('#PermissionSlipTemplateID option').each(function () {
		if (this.value == TemplateID) {
			exists = true;
			return false;
		}
	});
	if (exists) {
		$('#PermissionSlipTemplateID').val(TemplateID);
	}
	else {
		TemplateID = ""; //Reset to clear the values selected.
	}
	TemplateSelected(TemplateID);
}

function TemplateSelected(templateID) {

	//Get Template and populate fields
	//Clear values in case TemplateJSON doesn't come back with a value.
	$("#PermissionSlipCategoryID").val(null);
	$("#Name").val("");
	$("#Location").val("");
	$("#Cost").val(0);
	$("#RequireChaperone").prop("checked", false);
	$("#RequireChaperoneBackgroundCheck").prop("checked", false);
	if (templateID !== null) {
		var TemplateJSON = GetTemplateByID(templateID);
		if (TemplateJSON !== null) {
			$("#PermissionSlipCategoryID").val(TemplateJSON.CategoryID);
			$("#Name").val(TemplateJSON.Name);
			$("#Location").val(TemplateJSON.Location);
			$("#Cost").val(TemplateJSON.Cost);
			$("#RequireChaperone").prop("checked", TemplateJSON.RequireChaperone);
			$("#RequireChaperoneBackgroundCheck").prop("checked", TemplateJSON.RequireChaperoneBackgroundCheck);
		}
	}
}