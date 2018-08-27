$(function () {
	$('#searchButton').click(function () {
		MVCGrid.setAdditionalQueryOptions('TeacherSearchResults',
			{
				SchoolID: $('#SchoolID').val(),
				ClassID: $('#ClassRoomID').val(),
				StudentID: $('#StudentID').val(),
				PermissionSlipName: $('#PermissionSlipName').val(),
				StartDate: $('#EventStartDate').val(),
				EndDate: $('#EventEndDate').val(),
				ShowResults: true
			});

		MVCGrid.reloadGrid('TeacherSearchResults')
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
