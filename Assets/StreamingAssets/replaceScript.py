from sys import argv
import os

def multi_replace(search, replace, path):
    """Replace search with replace in all filenames
    and file contents in directory path.
    @type   search: string
    @param  search: The old string.
    @type   replace: string
    @param  replace: The new string.
    @type   path: string
    @param  path: The path in which files area.
    @rtype: boolean
    @returns: True or False. Also print a msg to the console.
    """
    counter_contents = 0
    counter_names = 0
    alteredFiles = []
    if not os.path.exists(path):
        print ('Path does not exist')
        return False
    for dirpath, dirs, files in os.walk(path):
        for filename in files:
            if "xml" in filename:
                print('process '+dirpath+'/'+filename)
                # replace contents
                indata = open(os.path.join(dirpath, filename)).read()
                if search in indata:
                    new = indata.replace(search, replace)
                    output = open(os.path.join(dirpath, filename), "w")
                    output.write(new)
                    counter_contents +=1
                    alteredFiles.append(dirpath+'/'+filename)
            
    print (str(counter_contents)+' files contents altered')
    for x in alteredFiles:
        print(x)
    return True

multi_replace("script name", "script outputLine", ".")
