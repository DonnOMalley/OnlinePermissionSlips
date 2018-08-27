$(function () {
	$('#searchButton').click(function () {
		var size = {
			width: window.innerWidth || document.body.clientWidth,
			height: window.innerHeight || document.body.clientHeight
		}

		if (size.width <= 600) {
			MVCGrid.setAdditionalQueryOptions('GuardianSearchResultsMobile',
				{
					SchoolID: $('#SchoolID').val(),
					ClassID: $('#ClassRoomID').val(),
					StudentID: $('#StudentID').val(),
					PermissionSlipName: $('#PermissionSlipName').val(),
					StartDate: $('#EventStartDate').val(),
					EndDate: $('#EventEndDate').val(),
					ApprovalStatusID: $('#ApprovalStatusID').val(),
					ShowResults: true
				});
			MVCGrid.reloadGrid('GuardianSearchResultsMobile')
		}
		else {
			MVCGrid.setAdditionalQueryOptions('GuardianSearchResults',
				{
					SchoolID: $('#SchoolID').val(),
					ClassID: $('#ClassRoomID').val(),
					StudentID: $('#StudentID').val(),
					PermissionSlipName: $('#PermissionSlipName').val(),
					StartDate: $('#EventStartDate').val(),
					EndDate: $('#EventEndDate').val(),
					ApprovalStatusID: $('#ApprovalStatusID').val(),
					ShowResults: true
				});
			MVCGrid.reloadGrid('GuardianSearchResults')
		}

		return false;
	});
});

$(function () {
	$('#EventStartDate').datetimepicker({
		sideBySide: false
	});
});

$(function () {
	$('#EventEndDate').datetimepicker({
		sideBySide: false
	});
});
