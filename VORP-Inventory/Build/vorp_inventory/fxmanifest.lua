game 'rdr3'
fx_version 'adamant'
rdr3_warning 'I acknowledge that this is a prerelease build of RedM, and I am aware my resources *will* become incompatible once RedM ships.'


ui_page 'html/ui.html'


client_scripts{
  'vorpinventory_cl.net.dll',
  'nuistuff.lua',
  'client/client.lua'
}
server_scripts {
  'vorpinventory_sv.net.dll',
  'vorpInventoryAPI.lua',
  'invstuff.lua',
  'server/server.lua'
}


server_exports{'vorp_inventoryApi'} 

files{
  'Newtonsoft.Json.dll',
  'html/ui.html',
  'html/css/contextMenu.min.css',
  'html/css/jquery.dialog.min.css',
  'html/css/ui.min.css',
  'html/js/config.js',
  'html/js/contextMenu.min.js',
  'html/js/jquery.dialog.min.js',
  'html/fonts/crock.ttf',
  'html/img/bgPanel.png',
  'html/img/bgPanel1.png',
  'html/img/bgPanel2.png',
  'html/img/bg.png',
  'html/img/bgitem.png',
  'html/webfonts/*',
  'html/webfonts/**',
  'html/img/items/*',
  'html/img/items/**',
  'html/img/items/*.png',
  'html/img/items/**.png',
  'html/css/all.css',
  'html/css/all.min.css',
  'html/img/items/*.jpg',
  'html/img/items/**.jpg'
}