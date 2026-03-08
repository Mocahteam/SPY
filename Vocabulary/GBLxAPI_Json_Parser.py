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
def GenerateJson(workbookName, target):
    wb = xlrd.open_workbook(filename=workbookName)

    totalMap = {} # totalMap has keys in [Activity, Grade, Domain, Focus, etc]
    for ws in wb.sheets():
        if ws.name == "Notes": continue

        print("Loading " + ws.name +"..."),
        
        sectionMap = {} # sectionMap has keys in [Counting, Algebra, Energy, etc]

        nc = 0 # A column in Excel file
        uc = 1 # B column in Excel file
        dc = 2 # C column in Excel file

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

# Load user overrides
print("Loading user data...")
GenerateJson('GBLxAPI_Vocab_User.xls', 'GBLxAPI_Vocab_User.json')

print("All done! Move the generated Json file to Resources/Data to use the GBLxAPI vocabulary in your Unity project.")
