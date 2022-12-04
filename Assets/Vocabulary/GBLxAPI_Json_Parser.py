# -------------------------------------------------------------------------------------------------
# GBLxAPI_Json_Parser.py
# Project: GBLXAPI
# Created: 2018/07/21
# Copyright 2018 Dig-It! Games, LLC. All rights reserved.
# This code is licensed under the MIT License (See LICENSE.txt for details)
# -------------------------------------------------------------------------------------------------

import xlrd
import json
from jsonmerge import merge

# This function takes all of the GBLxAPI Vocabulary information in the workbook named workbookName
# and parses it to json, writing to a file with the name defined in target.
def GenerateJson(workbookName, target, nameCol, uriCol, descrCol):
    wb = xlrd.open_workbook(filename=workbookName)

    totalMap = {} # totalMap has keys in [Activity, Grade, Domain, Focus, etc]
    for ws in wb.sheets():
        if ws.name == "Notes": continue

        print("Loading " + ws.name +"..."),
        
        sectionMap = {} # sectionMap has keys in [Counting, Algebra, Energy, etc]

        # local variables to allow for column overrides
        nc = nameCol
        uc = uriCol
        dc = descrCol

        # override column values for specific manually populated sheets in the default file
        # for automatically populated sheets, the default file uses columns F, I, and BB. For manual population, it's much easier to use A, B, and C.
        # This should not affect the values for the user vocab, since this file uses A, B, and C already.
        if ws.name in ["Verb", "Activity", "Extension", "Grade"]:
            nc = 0 # A
            uc = 1 # B
            dc = 2 # C

        for row in range(1, ws.nrows): # indexing from 1 to skip header row
            itemMap = {} # itemMap has keys in [name, description, id]
            
            # force all values to lowercase for easy comparison
            name = ws.cell_value(row, nc).lower()
            uri = ws.cell_value(row, uc).lower()
            descr = ws.cell_value(row, dc).lower()
            
            # populate the map with the corresponding values
            itemMap['name'] = {}
            itemMap['description'] = {}
            
            itemMap['name']['en-US'] = name
            itemMap['id'] = uri
            itemMap['description']['en-US'] = descr

            sectionMap[name] = itemMap

        totalMap[ws.name.lower()] = sectionMap

        print("Done.")

    print("Generating Json file..."),
    with open(target, 'w') as write_file:
        json.dump(totalMap, write_file, sort_keys=True, indent=4, separators=(',', ': '))
        print("Success!")

print("Converting your data...")

# Load default vocabulary
# 5 == row F, 8 == row I, 53 == row BB in Excel
print("Loading default vocabulary...")
GenerateJson('GBLxAPI_Vocab_Default.xls', 'GBLxAPI_Vocab_Default.json', 5, 53, 8)

# Load user overrides
# 0 == row A, 1 == row B, 2 == row C in Excel
print("Loading user overrides...")
GenerateJson('GBLxAPI_Vocab_User.xls', 'GBLxAPI_Vocab_User.json', 0, 1, 2)

print("All done! Move the two generated Json files to Resources/Data to use the GBLxAPI vocabulary in your Unity project.")
