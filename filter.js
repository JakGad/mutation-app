var results = require("<path>\\mutation.mutation4.json")
const fs = require('fs');

var resTab = []

results.forEach(res => {
  res.CommitResults.forEach(commit => {
    commit.FileResults.forEach(file => {
      if(file.ParentTreeFragment &&file.OriginalTreeFragment && file.ParentTreeFragment.length < 30 && file.OriginalTreeFragment.length < 30 && !resTab.find(el => el.parent == file.ParentTreeFragment && el.original == file.OriginalTreeFragment)) {
        resTab = [...resTab, {parent: file.ParentTreeFragment, original: file.OriginalTreeFragment}]
      }
    })
  })
})

let data = JSON.stringify(resTab, null, 2);
fs.writeFileSync('<path>', data);


console.log(resTab.length)