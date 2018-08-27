
var saveButton = document.getElementById('SaveButton');
var clearButton = document.getElementById('ClearButton');

clearButton.addEventListener("click", function () {
	signaturePad.fromDataURL();
	signaturePad.clear();
	$("#SignatureData").val(null);
	$("#Signature").val(null);
});

saveButton.addEventListener('click', function (event) {
	if (!signaturePad.isEmpty()) {
		var data = signaturePad.toDataURL('image/png');
		$("#SignatureData").val(data);
		form.submit();
	}
	else {
		$("#SignatureData").val("data:image/png;base64," + $("#ExistingSignature").val());
	}
});

var resize = function () {
	var dataURL = signaturePad.toDataURL();
	var canvas = document.getElementById('signature-pad');
	var ratio = Math.max(window.devicePixelRatio || 1, 1);
	var parentWidth = canvas.parentElement.clientWidth - 40;
	canvas.width = parentWidth * ratio;
	canvas.height = canvas.height * ratio;
	canvas.getContext("2d").scale(ratio, ratio);
	if (!signaturePad.isEmpty()) {
		// If you don't check that the pad is empty then writing data (even if that data is a blank image) will make it think you have data in there!
		signaturePad.fromDataURL(dataURL);
	}
	var parentWidth = canvas.parentElement.clientWidth - 40;
	canvas.setAttribute("width", parentWidth);
	canvas.setAttribute("height", 200);

};

window.addEventListener('resize', function () { resize(); });
resize();