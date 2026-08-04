window.showDeleteConfirm = function () {
    return Swal.fire({
        title: "Are you sure delete this record?",
        icon: "warning",
        timer: 10000,
        timerProgressBar: true,
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then(result => result.isConfirmed);
};
window.showActiveConfirm = function () {
    return Swal.fire({
        title: "Are you sure active this record?",
        icon: "warning",
        timer: 10000,
        timerProgressBar: true,
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes,activeted it!"
    }).then(result => result.isConfirmed);
};
window.showUpdateConfirm = function () {
    return Swal.fire({
        title: "Are you sure you want to  update this record?",
        icon: "warning",
        timer: 10000,
        timerProgressBar: true,
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes,updated it!"
    }).then(result => result.isConfirmed);
};

window.showSelectRecordAlert = function () {
    Swal.fire({
        icon: 'warning',
        title: 'Please select a record !',
        timer: 10000,
        timerProgressBar: true,
        confirmButtonText: 'OK',
        allowOutsideClick: true,
        allowEscapeKey: true
    });
};

window.showPrintConfirm = function () {
    return Swal.fire({
        title: "Choose Print Option",
        text: "Select how you want to export the record.",
        icon: "info",
        timer: 10000,
        timerProgressBar: true,
        showDenyButton: true,
        showCancelButton: false,

        confirmButtonText: "Print PDF",
        denyButtonText: "Print Excel",

        confirmButtonColor: "#3085d6",
        denyButtonColor: "#28a745",

        allowOutsideClick: true,
        allowEscapeKey: true,
        allowEnterKey: false,
        backdrop: true
    });
};

window.downloadPdf = function (base64, fileName) {
    const link = document.createElement("a");
    link.href = "data:application/pdf;base64," + base64;
    link.download = fileName;
    link.click();
};
function downloadExcel(base64, fileName) {
    const link = document.createElement('a');
    link.href = "data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64," + base64;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
// window.viewPdf = function (base64Pdf) {

//     const newTab = window.open("about:blank", "_blank");
//     newTab.document.body.innerHTML =
//         "<iframe width='100%' height='100%' src='data:application/pdf;base64,"
//         + base64Pdf + "'></iframe>";
//     newTab.document.close();

// }
window.viewPdf = function (base64Pdf) {

    const byteCharacters = atob(base64Pdf);
    const byteNumbers = new Array(byteCharacters.length);

    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }

    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: "application/pdf" });

    const url = URL.createObjectURL(blob);

    window.open(url, "_blank");
};
window.openDatePickerFromWrapper = (wrapper) => {
    if (!wrapper) return;

    const input = wrapper.querySelector("input[type='date']");
    if (input?.showPicker) {
        input.showPicker();
    } else if (input) {
        input.focus();
    }
};
window.openDatePicker = (el) => {
    el.showPicker();
};

window.showRegistrationSuccess = async function (message) {
    const result = await Swal.fire({
        html: message,
        icon: "success",
        timer: 20000,
        timerProgressBar: true,
        showCancelButton: true,
        confirmButtonText: "Yes",
        cancelButtonText: "No"
    });
    if (result.dismiss === Swal.DismissReason.timer) {
        return false;
    }

    return result.isConfirmed;
}
window.scrollToElement = (id) => {
    var element = document.getElementById(id);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};
window.showProcessingAlert = () => {

    Swal.fire({
        title: 'Processing...',
        html: 'Please wait...',
        allowOutsideClick: false,
        allowEscapeKey: false,

        didOpen: () => {
            Swal.showLoading();
        }
    });

};

window.closeSwal = () => {
    Swal.close();
};
window.openFilePicker = function (id) {
    document.getElementById(id).click();
};

window.getImageSize = function (id) {
    return new Promise((resolve) => {

        const input = document.getElementById(id);

        if (!input || !input.files || input.files.length === 0) {
            resolve({ width: 0, height: 0 });
            return;
        }

        const file = input.files[0];

        const img = new Image();

        img.onload = function () {
            resolve({
                width: img.width,
                height: img.height
            });
        };

        img.src = URL.createObjectURL(file);
    });
};
window.loadStudentChart = function (data, isIncrease) {

    if (!data || data.length === 0)
        return;

    const width = 90;
    const height = 40;
    const step = width / (data.length - 1);

    const max = Math.max(...data);
    const min = Math.min(...data);

    let points = "";

    data.forEach((v, i) => {

        let y = height;

        if (max !== min)
            y = height - ((v - min) * height / (max - min));

        points += `${i * step},${y} `;
    });

    const color = isIncrease ? "#22c55e" : "#ef4444";

    document.getElementById("studentGraph").innerHTML = `
        <svg width="90" height="40" viewBox="0 0 90 40">
            <polyline
                points="${points}"
                fill="none"
                stroke="${color}"
                stroke-width="3"
                stroke-linecap="round"
                stroke-linejoin="round">
            </polyline>
        </svg>`;
}
window.initSplitter = () => {

    const splitter = document.getElementById("splitter");
    const right = document.getElementById("rightPanel");

    let dragging = false;

    splitter.onmousedown = function () {
        dragging = true;
        document.body.style.cursor = "col-resize";
        document.body.style.userSelect = "none";
    };

    document.onmousemove = function (e) {

        if (!dragging) return;

        const newWidth = window.innerWidth - e.clientX;

        if (newWidth >= 250 && newWidth <= 700) {
            right.style.width = newWidth + "px";
        }
    };

    document.onmouseup = function () {
        dragging = false;
        document.body.style.cursor = "";
        document.body.style.userSelect = "";
    };
};
(function () {
    function updateState(el) {
        var wrap = el.closest('.form-box-group');
        if (!wrap) return;
        var hasValue;
        if (el.type === 'checkbox' || el.type === 'radio') {
            hasValue = el.checked;
        } else {
            hasValue = el.value !== null && el.value !== '' && el.value !== '0';
        }
        wrap.classList.toggle('has-value', hasValue);
    }

    function initAll() {
        document.querySelectorAll('.floating-input').forEach(updateState);
    }

    // User type kare ya select/checkbox/radio badle - turant update
    document.addEventListener('input', function (e) {
        if (e.target.classList && e.target.classList.contains('floating-input')) updateState(e.target);
    }, true);

    document.addEventListener('change', function (e) {
        if (e.target.classList && e.target.classList.contains('floating-input')) updateState(e.target);
    }, true);

    // Blazor jab naya section (Concession / Sibling) render kare, uske fields bhi pakdo
    var observer = new MutationObserver(initAll);
    observer.observe(document.body, { childList: true, subtree: true });

    // Blazor edit-mode me values ko C# se load karta hai (LoadData/getdatamodepass),
    // ye DOM 'input'/'change' event fire nahi karta, isliye thodi der poll karke
    // already-filled / already-checked fields ka label bhi turant upar kar do.
    var pollCount = 0;
    var poller = setInterval(function () {
        initAll();
        pollCount++;
        if (pollCount > 40) clearInterval(poller); // ~10 sec baad band
    }, 250);

    initAll();
})();

let sessionChart = null;
let followupChart = null;
let sourceChart = null;
let sourceDataGlobal = [];
let hiddenSources = [];
let pipelineDataGlobal = [];
let hiddenStages = [];

window.renderDashboardCharts = function (sessionData, sourceData, pipelineData) {

    // ================= SESSION COMPARISON =================
    let displaySessions = sessionData.slice(-5);
    const sessionCanvas = document.getElementById("sessionComparisonChart");

    if (sessionCanvas) {

        if (sessionChart)
            sessionChart.destroy();

        sessionChart = new Chart(sessionCanvas.getContext("2d"), {

            type: 'bar',

            data: {

                labels: displaySessions.map(x => x.sessionName),

                datasets: [
                    {
                        label: 'Enquiry',
                        data: displaySessions.map(x => x.totalEnquiry),
                        backgroundColor: '#3b82f6'
                    },
                    {
                        label: 'Application',
                        data: displaySessions.map(x => x.totalApplication),
                        backgroundColor: '#10b981'
                    },
                    {
                        label: 'Registration',
                        data: displaySessions.map(x => x.totalRegistration),
                        backgroundColor: '#f59e0b'
                    },
                    {
                        label: 'Admission',
                        data: displaySessions.map(x => x.totalAdmission),
                        backgroundColor: '#a855f7'
                    }
                ]

            },

            options: {

                responsive: true,
                maintainAspectRatio: false,

                plugins: {
                    legend: {
                        position: 'top'
                    }
                },

                scales: {

                    y: {
                        beginAtZero: true
                    }

                }

            }

        });
        console.log(sourceData);
    }

    // ================= FOLLOWUP =================
    pipelineDataGlobal = pipelineData;

    const followCanvas = document.getElementById("followupDoughnutChart");

    if (followCanvas) {

        if (followupChart)
            followupChart.destroy();

        followupChart = new Chart(followCanvas.getContext("2d"), {

            type: 'doughnut',

            data: {

                // labels: pipelineData.map(x => x.stage),

                datasets: [

                    {
                        data: pipelineData.map(x => x.totalCount),

                        backgroundColor: pipelineData.map(x => getColor(x.stage))

                    }

                ]

            },

            options: {

                responsive: true,

                maintainAspectRatio: false,

                plugins: {

                    legend: {

                        position: 'bottom'

                    }

                }

            }

        });

    }

    sourceDataGlobal = sourceData;


    const sourceCanvas = document.getElementById("sourcePieChart");


    if (sourceCanvas) {

        if (sourceChart)
            sourceChart.destroy();


        sourceChart = new Chart(
            sourceCanvas.getContext("2d"),
            {
                type: 'doughnut',

                data: {
                    labels: sourceData.map(x => x.sourceName),

                    datasets: [
                        {
                            data: sourceData.map(x => x.totalCount),

                            backgroundColor: sourceData.map(x => getColor(x.sourceName))
                        }
                    ]
                },

                options: {

                    responsive: true,

                    cutout: '55%',

                    plugins: {

                        legend: {
                            display: false  
                        }

                    }

                }
            }
        );
    }
}

window.toggleSource = function (sourceName) {

    let index = hiddenSources.indexOf(sourceName);

    if (index > -1) {
        hiddenSources.splice(index, 1);
    }
    else {
        hiddenSources.push(sourceName);
    }


    let filteredData = sourceDataGlobal.filter(x =>
        !hiddenSources.includes(x.sourceName)
    );


    if (sourceChart) {

        sourceChart.data.labels =
            filteredData.map(x => x.sourceName);

        sourceChart.data.datasets[0].data =
            filteredData.map(x => x.totalCount);

        sourceChart.data.datasets[0].backgroundColor =
            filteredData.map(x => getColor(x.sourceName));

        sourceChart.update();
    }

    document.querySelectorAll(".source-row")
        .forEach(row => {

            let name = row.getAttribute("data-source");

            if (hiddenSources.includes(name)) {
                row.classList.add("source-disabled");
            }
            else {
                row.classList.remove("source-disabled");
            }

        });

};

window.togglePipeline = function (stage) {

    let index = hiddenStages.indexOf(stage);

    if (index > -1) {
        hiddenStages.splice(index, 1);
    } else {
        hiddenStages.push(stage);
    }

    let filteredData = pipelineDataGlobal.filter(x =>
        !hiddenStages.includes(x.stage)
    );

    if (followupChart) {

        followupChart.data.labels =
            filteredData.map(x => x.stage);

        followupChart.data.datasets[0].data =
            filteredData.map(x => x.totalCount);

        followupChart.data.datasets[0].backgroundColor =
            filteredData.map(x => getColor(x.stage));

        followupChart.update();
    }

    document.querySelectorAll(".pipeline-row")
        .forEach(row => {

            let stageName = row.getAttribute("data-stage");

            if (hiddenStages.includes(stageName)) {
                row.classList.add("source-disabled");
            } else {
                row.classList.remove("source-disabled");
            }

        });
};
function getColor(name) {

    switch (name) {

        case "Facebook":
        case "Enquiry":
            return "#3b82f6";

        case "Instagram":
        case "Application":
            return "#10b981";

        case "Google":
        case "Registration":
            return "#f59e0b";

        case "Walking":
        case "Admission":
            return "#a855f7";

        case "Reference":
            return "#ef4444";

        case "Others":
        case "Walking/Others":
            return "#14b8a6";

        default:
            return "#6c757d";
    }
}

window.renderSessionWiseChart = function (sessionData, chartType) {

    let displaySessions = sessionData.slice(0, 5).reverse();

    const sessionCanvas = document.getElementById("sessionComparisonChart");

    if (!sessionCanvas)
        return;

    if (sessionChart)
        sessionChart.destroy();

    sessionChart = new Chart(sessionCanvas.getContext("2d"), {

        type: chartType,

        data: {
            labels: displaySessions.map(x => x.sessionName),

            datasets: [
                {
                    label: 'Enquiry',
                    data: displaySessions.map(x => x.totalEnquiry),
                    backgroundColor: '#3b82f6'
                },
                {
                    label: 'Application',
                    data: displaySessions.map(x => x.totalApplication),
                    backgroundColor: '#10b981'
                },
                {
                    label: 'Registration',
                    data: displaySessions.map(x => x.totalRegistration),
                    backgroundColor: '#f59e0b'
                },
                {
                    label: 'Admission',
                    data: displaySessions.map(x => x.totalAdmission),
                    backgroundColor: '#a855f7'
                }
            ]
        },

        options: {
            responsive: true,
            maintainAspectRatio: false,

            plugins: {
                legend: {
                    position: 'top'
                }
            },

            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}
window.renderMonthWiseChart = function (data, chartType) {

    if (sessionChart)
        sessionChart.destroy();

    const ctx = document.getElementById("sessionComparisonChart");

    sessionChart = new Chart(ctx, {

        type: chartType,

        data: {

            labels: data.map(x => x.monthName),

            datasets: [
                {
                    label: 'Enquiry',
                    data: data.map(x => x.totalEnquiry),
                    backgroundColor: '#3b82f6' // Blue
                },
                {
                    label: 'Registration',
                    data: data.map(x => x.registration),
                    backgroundColor: '#f59e0b' // Orange
                },
                {
                    label: 'Pending',
                    data: data.map(x => x.pendingRegistration),
                    backgroundColor: '#10b981' // Green
                }
            ]
        },

        options: {
            responsive: true,
            maintainAspectRatio: false
        }
    });
}
window.renderClassWiseChart = function (data, chartType) {
    console.log(data[0]);
    if (sessionChart)
        sessionChart.destroy();

    const ctx = document.getElementById("sessionComparisonChart");

    sessionChart = new Chart(ctx, {

        type: chartType,

        data: {

            labels: data.map(x => x.classCode),

            datasets: [
                {
                    label: 'Enquiry',
                    data: data.map(x => x.totalEnquiry),
                    backgroundColor: '#3b82f6' // Blue
                },
                {
                    label: 'Registration',
                    data: data.map(x => x.registration),
                    backgroundColor: '#f59e0b' // Orange
                },
                {
                    label: 'Admission',
                    data: data.map(x => x.totalAdmission),
                    backgroundColor: '#a855f7' // Purple
                }
            ]
        },

        options: {
            responsive: true,
            maintainAspectRatio: false
        }
    });
}
