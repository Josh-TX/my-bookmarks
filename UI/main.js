_allBookmarks = []
_canEdit = false;
_passKey = "";
_isEditing = false;
init();

async function init(){
    _passKey = localStorage["passKey"];
    _canEdit = await loadCanEdit(_passKey);
    _allBookmarks = await loadBookmarks();
    renderBookmarks(_allBookmarks);
}

async function updateBookmarks(){
    await saveBookmarks(_allBookmarks, _passKey);
    _allBookmarks = await loadBookmarks();
    await renderBookmarks(_allBookmarks, _isEditing);
}

async function loadCanEdit(passKey) {
    var response = await fetch("/api/bookmarks/can-edit?passkey=" + encodeURI(passKey));
    result = await response.text();
    return result.toLowerCase() == "true";
}

async function loadBookmarks() {
    var response = await fetch("/api/bookmarks");
    return await response.json();
}

async function saveBookmarks(allBookmarks, passKey) {
    await fetch("/api/bookmarks?passkey=" + encodeURI(passKey), { 
        method: "PUT", 
        body: JSON.stringify(allBookmarks), 
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
    });
}

function renderBookmarks(allBookmarks, isEditing){
    var container = document.getElementById("bookmark-container");
    container.innerHTML = "";
    for (var bookmark of allBookmarks){
        container.appendChild(createBookmarkElement(bookmark, isEditing));
    }
    if (isEditing){
        var div = el("div");
        var h2 = el("h2", "Add Bookmark");
        var b = el("button");
        b.appendChild(h2);
        b.addEventListener("click", e => {
            var url = prompt("New Bookmark Url");
            if (url == null){
                return;
            }
            if (!/^https?:\/\//.test(url)){
                alert("url must start with http:// or https://");
                return;
            }
            var name = prompt ("New Bookmark Name");
            if (name == null){
                return;
            }
            _allBookmarks.push({name: name, url: url});
            updateBookmarks();
        })
        div.appendChild(b);
        b.classList.add("col-span-2")
        div.classList.add("bookmark")
        container.append(div);
    }
}

function createBookmarkElement(bookmark, isEditing){
    var img = el("img");
    img.classList.add("icon")
    img.setAttribute("src", `/api/bookmarks/${bookmark.id}/icon`)
    var title = el("div", bookmark.name);
    title.classList.add("title" ,"ellipsis")
    var url = el("div", bookmark.url);
    url.classList.add("text-muted", "small", "ellipsis")
    var innerDiv = el("div");
    innerDiv.appendChild(title);
    innerDiv.appendChild(url);
    innerDiv.classList.add("bookmark-info")
    var a = isEditing ? el("div") : el("a");
    a.appendChild(img);
    a.appendChild(innerDiv);
    if (!isEditing){
        a.setAttribute("href", bookmark.url);
    }
    if (isEditing){
        var editDiv = el("div");
        var b1 = el("button", "re-order");
        b1.addEventListener("click", e => {
            var input = prompt ("Enter new position (number from 0 to " + _allBookmarks.length + ")");
            if (input == null){
                return;
            }
            var index = parseInt(input);
            if (index >= 0 && index <= _allBookmarks.length){
                _allBookmarks = _allBookmarks.filter(z => z != bookmark);
                _allBookmarks.splice(index, 0, bookmark);
                updateBookmarks();
            }
        })
        var b2 = el("button", "edit name");
        b2.addEventListener("click", e => {
            var input = prompt("Enter new name", bookmark.name);
            if (!input){
                return;
            }
            bookmark.name = input;
            updateBookmarks();
        });
        var b3 = el("button", "edit url");
        b3.addEventListener("click", e => {
            var input = prompt("Enter new url", bookmark.url);
            if (!input){
                return;
            }
            if (!/^https?:\/\//.test(input)){
                alert("url must start with http:// or https://");
                return;
            }
            bookmark.url = input;
            updateBookmarks();
        });
        var b4 = el("button", "delete");
        b4.addEventListener("click", e => {
            var input = confirm("Delete Bookmark '" + bookmark.name + "'?");
            if (!input){
                return;
            }
            _allBookmarks = _allBookmarks.filter(z => z != bookmark);
            updateBookmarks();
        });
        editDiv.appendChild(b1);
        editDiv.appendChild(b2);
        editDiv.appendChild(b3);
        editDiv.appendChild(b4);
        editDiv.classList.add("col-span-2")
        a.appendChild(editDiv);
    }
    a.classList.add("bookmark")
    return a;
}

function el(tag, content){
    var el = document.createElement(tag);
    if (content && typeof content == "string"){
        el.innerText = content
    }
    return el;
}

async function startEdit(){
    while (!_canEdit){
        _passKey = prompt("enter the PassKey");
        if (_passKey == null){
            break;
        }
        _canEdit = await loadCanEdit(_passKey);
    }
    if (_canEdit){
        localStorage["passKey"] = _passKey;
        _isEditing = true;
        renderBookmarks(_allBookmarks, _isEditing);
    }
}