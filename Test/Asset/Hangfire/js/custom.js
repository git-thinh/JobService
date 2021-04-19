window.Hangfire.config = {
    consolePollUrl: '',
    consolePollInterval: 0
};

var url_ws = (location.protocol == 'https:' ? 'wss' : 'ws') + '://' + location.host + '/ws'
console.log("CUSTOM JS ??????????????? ", url_ws);
// Create WebSocket connection.
const socket = new WebSocket(url_ws);

// Connection opened
socket.addEventListener('open', function (event) {
    socket.send('Hello Server!');
});

// Listen for messages
socket.addEventListener('message', function (event) {
    var data = event.data || '';
    console.log('Message from server ', data);
    if (data != null && data.length > 0
        && location.href.indexOf(event.data) != -1) location.reload();
});

function __logout() {
    location.href = '/logout';
}


function toggleMenu(e) {
    //alert(1)
    var wrap = document.getElementById('wrap');
    if (wrap) {
        var el = wrap.firstElementChild.firstElementChild;
        var rec = el.getBoundingClientRect();
        console.log(rec);

        var div = document.getElementById('__cs_menu');
        if (div == null) {
            div = document.createElement('div');
            div.setAttribute('id', '__cs_menu');
            div.style.left = (Math.round(rec.right) - 200) + 'px';
            div.style.display = 'inline-block';
            div.innerHTML = '<button onclick="__logout()">Sign Out</button>'
            document.body.appendChild(div);
        } else {
            if (div.style.display == 'none')
                div.style.display = 'inline-block';
            else
                div.style.display = 'none';
        }
    }
}

$(document).ready(function () {

    var menu = document.querySelector('#wrap .navbar-collapse .nav.navbar-nav.navbar-right');
    if (menu) {
        //menu.innerHTML = '<li style="display:inline-block"><a href="#" onclick="toggleMenu(event)"><span class="glyphicon glyphicon-menu-hamburger"></span> Menu</a></li>';
        menu.innerHTML = '<li style="display:inline-block"><a href="/logout"><span class="glyphicon glyphicon-log-out"></span> Sing Out</a></li>';
    }

    var mn = document.querySelector('#wrap .navbar-collapse .nav.navbar-nav:not(.navbar-right)');
    if (mn) {
        var li1 = document.createElement('li');
        li1.innerHTML = '<a href="#" onclick="reloadJobs(event)"><span class="glyphicon glyphicon-refresh"></span> Reload Jobs</a>';
        mn.appendChild(li1);
    }

    var aHome = document.querySelector('#wrap .container .navbar-header a');
    if (aHome) aHome.setAttribute('href', '/');

    document.querySelectorAll('h3').forEach(function (el) {
        if (el.innerHTML == 'Parameters') {
            el.innerHTML = '';
            el.style.display = 'none';
        }
    });



    window.addEventListener('click', function (e) {
        
    });
});


function reloadJobs() {
    fetch('/api/job/refresh').then(function (r) { return r.json(); }).then(function (r) {
        location.href = '/admin/recurring';
    })
}