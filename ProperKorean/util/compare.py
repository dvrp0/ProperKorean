import json, os

with open("DLC3.json", "r", encoding="utf-8-sig") as f:
    new = json.load(f)["strings"].keys()

with open("../Resources/output-korean.json", "r", encoding="utf-8") as f:
    old = json.load(f)["strings"].keys()

for key in new:
    if key in old:
        print(key)