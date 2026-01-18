vim.opt.path:append({ ".", "./**", "Blocks", "Blocks/**", "Services", "Services/**", "X11", "X11/**" })
vim.opt.wildignore:append({
  "*/obj/*",
  "*/obj/**",
  "*/bin/*",
  "*/bin/**",
  "*/.git/*",
  "*/.git/**",
  "*/.vs/*",
  "*/.vs/**",
})
