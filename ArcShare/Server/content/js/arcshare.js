var selected = [];

console.log("ArcShare.js has been loaded");

function BytesNumToString(b) {
	var si = true;
	var thresh = si ? 1000 : 1024;
	if (b < thresh) return b + ' B';
	var units = si ? ['kB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'] : ['KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB'];
	var u = -1;
	do {
		b /= thresh;
		++u;
	} while (b >= thresh);
	return b.toFixed(1) + ' ' + units[u];
}

function onFilePickerSelected() {
	var files = document.getElementById("filePicker").files;
	for (var i = 0; i < files.length; i++) {
		selected.push(files[i]);
	}
	console.log(files.length + " files has been selected");
	viewUpdate();
}

function viewUpdate() {
	console.log("called viewUpdate();");
	var table = document.getElementById("table");

	$(".clickable-row").each(function () {
		$(this).remove();
	});

	var progressBar = document.getElementById("progressbar");
	progressBar.style.width = "0%";
	progressBar.setAttribute("aria-valuenow", "0");
	var progressText = document.getElementById("percentageNum");
	progressText.innerHTML = "0";

	var totalSize = 0;

	if (selected.length === 0) {
		return;
	}

	for (var i = 0; i < selected.length; i++) {
		var file = selected[i];
		if (file) {
			totalSize += file.size;
			var sizeInStr = BytesNumToString(file.size);

			var row = table.insertRow(-1);
			row.classList.add("clickable-row");
			row.id = "row" + i;
			var id = row.insertCell(0);
			var name = row.insertCell(1);
			var size = row.insertCell(2);

			id.innerHTML = i;
			name.innerHTML = file.name;
			size.innerHTML = sizeInStr;

			console.log("Added :" + file.name);
		}
	}

	$(".clickable-row").click(function () {
		if ($(this).hasClass("bg-secondary"))
			$(this).removeClass('bg-secondary');
		else
			$(this).addClass('bg-secondary');
	});

	totalbytes = totalSize;
}

function onSelecteAll() {
	$(".clickable-row").each(function () {
		if (!$(this).hasClass("bg-secondary")) {
			$(this).addClass("bg-secondary");
		}
	});
}

function onDeselectAll() {
	$(".clickable-row").each(function () {
		if ($(this).hasClass("bg-secondary")) {
			$(this).removeClass("bg-secondary");
		}
	});
}

jQuery.fn.reverse = [].reverse;
function onRemoveClick() {
	$(".clickable-row").reverse().each(function () {
		if ($(this).hasClass("bg-secondary")) {
			var id = $(this).attr("id").replace("row", "");
			var num = Number(id);
			console.log("before: " + selected);
			selected.splice(num, 1);
			console.log("after: " + selected);
			viewUpdate();
		}
	});
}

function startUpload() {
	var form = new FormData();
	for (i = 0; i < selected.length; i++) {
		var file = selected[i];
		form.append("files", file);
	}
	var request = new XMLHttpRequest();
	request.addEventListener("load", uploadCompleted, false);
	request.addEventListener("error", uploadError, false);
	request.addEventListener("cancel", uploadCanceled, false);

	request.upload.addEventListener("progress", updateProgressChanged, false);

	request.open("POST", "/up");

	request.send(form);
}

function updateProgressChanged(event) {
	var progressBar = document.getElementById("progressbar");
	if (event.lengthComputable) {
		var percent = Math.round(event.loaded * 100 / event.total);
		progressBar.style.width = percent + "%";
		progressBar.setAttribute("aria-valuenow", percent);
		document.getElementById("percentageNum").innerHTML = percent;
	} else {
		progressBar.style.width = "100%";
		progressBar.setAttribute("aria-valuenow", 100);
		//document.getElementById("percentageNum").innerHTML = 100;
	}
}

function uploadCompleted(event) {

}

function uploadError(event) {
	alert("We've dropped into an error while sending.");
}

function uploadCanceled(event) {
	alert("Transfering was canceled");
}