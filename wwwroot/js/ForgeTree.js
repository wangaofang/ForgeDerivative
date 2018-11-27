$(document).ready(function () {
    // first, check if current visitor is signed in
    jQuery.ajax({
        url: '/api/forge/oauth/token',
        success: function (res) {
            // yes, it is signed in...
            $('#signOut').show();
            $('#refreshHubs').show();

            // prepare sign out
            $('#signOut').click(function () {
                $('#hiddenFrame').on('load', function (event) {
                    location.href = '/api/forge/oauth/signout';
                });
                $('#hiddenFrame').attr('src', 'https://accounts.autodesk.com/Authentication/LogOut');
                // learn more about this signout iframe at
                // https://forge.autodesk.com/blog/log-out-forge
            })

            // and refresh button
            $('#refreshHubs').click(function () {
                $('#userHubs').jstree(true).refresh();
            });

            // finally:
            prepareUserHubsTree();
            showUser();
        }
        //,error: function (res) {
        //    alert(res);
        //}

    });

    $('#autodeskSigninButton').click(function () {
        jQuery.ajax({
            url: '/api/forge/oauth/url',
            success: function (url) {
                location.href = url;
            }
        });
    })
});

function prepareUserHubsTree() {
    $('#userHubs').jstree({
        'core': {
            'themes': { "icons": true },
            'multiple': false,
            'data': {
                "url": '/api/forge/datamanagement',
                "dataType": "json",
                'cache': false,
                'data': function (node) {
                    $('#userHubs').jstree(true).toggle_node(node);
                    return { "id": node.id };
                }
            }
        },
        'types': {

            'default': {
                'icon': 'glyphicon glyphicon-question-sign'
            },

            '#': {
                'icon': 'glyphicon glyphicon-user'
            },
            'hubs': {
                'icon': 'https://github.com/Autodesk-Forge/bim360appstore-data.management-nodejs-transfer.storage/raw/master/www/img/a360hub.png'
            },
            'personalHub': {
                'icon': 'https://github.com/Autodesk-Forge/bim360appstore-data.management-nodejs-transfer.storage/raw/master/www/img/a360hub.png'
            },
            'bim360Hubs': {
                'icon': 'https://github.com/Autodesk-Forge/bim360appstore-data.management-nodejs-transfer.storage/raw/master/www/img/bim360hub.png'
            },
            'bim360projects': {
                'icon': 'https://github.com/Autodesk-Forge/bim360appstore-data.management-nodejs-transfer.storage/raw/master/www/img/bim360project.png'
            },
            'a360projects': {
                'icon': 'https://github.com/Autodesk-Forge/bim360appstore-data.management-nodejs-transfer.storage/raw/master/www/img/a360project.png'
            },
            'items': {
                'icon': 'glyphicon glyphicon-file'
            },
            'folders': {
                'icon': 'glyphicon glyphicon-folder-open'
            },
            'versions': {
                'icon': 'glyphicon glyphicon-time'
            },
            'unsupported': {
                'icon': 'glyphicon glyphicon-ban-circle'
            }
        },
        "plugins": ["contextmenu", "types", "state", "sort"],
        "state": { "key": "autodeskHubs" },// key restore tree state

        "contextmenu": {
            "items": customMenu
        }

    }).bind("activate_node.jstree", function (evt, data) {
        if (data != null && data.node != null && data.node.type == 'versions') {
            $("#forgeViewer").empty();
            var urn = data.node.id;
            launchViewer(urn);
        }
    });
}

function customMenu($node) {
    var items = {};
    if (this.get_type($node) === "versions") {
        items = {
            'Download': {
                'label': 'Download',
                "action": function (obj) {
                    var id = $.jstree.reference(obj.reference).get_node(obj.reference).id;
                    alert(id);
                    $.ajax({
                         type:"post",
                         url:"/api/FielDownload/download",
                         data:JSON.stringify({urn:id}),   
                         dataType: "json",                     
                         contentType: "application/json; charset=utf-8",
                         success:function(response){
                             alert(Response);
                         }
                    });

                    // $.post("/api/FielDownload/download", $.param({urn: id})).done(function(response){
                    //     //save the token in local storage
                    //     alert(response);
                    //     //...
                    // }).fail(function(res){
                    //     alert(res);
                    // });
                }
            },
            'Delete': {
                'label': 'Delete',
                "action": function (obj) {
                   
                }
            }
        }
    }
    else if(this.get_type($node) === "folders")
    {
        items = {
            'Upload': {
                'label': 'Upload',
                "action": function (obj) {
                    
                }
            },

            'Delete': {
                'label': 'Delete',
                "action": function (obj) {
                    
                }
            }
        }
    }
        return items;    
}



    function showUser() {
        jQuery.ajax({
            url: '/api/forge/user/profile',
            success: function (profile) {
                var img = '<img src="' + profile.picture + '" height="30px">';
                $('#userInfo').html(img + profile.name);
            }
        });
    }