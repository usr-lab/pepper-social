'''
This script was used to calculate/generate the performance table.

To use it follow steps 1-4.
'''



'''
#
#   Step 1: Enter your results
#
'''
results = {
            'Random'                        : -1.175555412,
            'Vector + LSTM'                 : 0.8806,
            'Vector + FF'                   : -1.0,
            'CameraOnly + our + FF'         : -1.0,
            'CameraOnly + our + LSTM'       : -0.01368,
            'CameraOnly + conv + FF'        : -1.0,
            'CameraOnly + conv + LSTM'      : 0.1332,
            'CameraSpeed + our + FF'        : -1.0,
            'CameraSpeed + our + LSTM'      : -1.0,
            'CameraSpeed + conv + FF'       : -1.0,
            'CameraSpeed + conv + LSTM'     : -1.0,
           }

'''
#
#   Step 2: tell if you want latex-format, and if you want something printed between the entries.
#
'''
print_latex_table = False
latex_line_separator = '' #use '\\hline' to have a line between all entries

'''
#
#   Step 3: Decide lower and an upper baseline.
#
'''
lower = results['Random']
upper = results['Vector + LSTM']


'''
#
#   Step 4: Run the script!
#
'''


#####
##### Just code below . . .
#####

if not print_latex_table:
    for name in sorted(results):
        value = results[name]
        relative = 100 * (value - lower) / (upper - lower)
        print( "{:30}\t\t{}\t{}%".format(name,round(value,2),round(relative,2)) )
else:
    best_val = sorted(results.items(), key=lambda x:x[1], reverse=True)[0][1]
    print(
        """\\begin{table}[H]
\\begin{center}
\\begin{tabular}{||l | c| c ||}
\\hline
\\textbf{Model} & Reward & Percentage \\\\ [0.5ex]
\\hline\\hline"""
        )
    for name in sorted(results):
        value = results[name]
        relative = 100 * (value - lower) / (upper - lower)
        if value == best_val:
            print( "{:30} & \\textbf{{{}}} & \\textbf{{{}}}\\% \\\\ {}".format(name,round(value,2),round(relative,2), latex_line_separator) )
        print( "{:30} & {} & {}\\% \\\\ {}".format(name,round(value,2),round(relative,2), latex_line_separator) )
    print(
        '''\\hline
\\end{tabular}
\\end{center}
\\caption{Results of the different configurations.}
\\label{results}
\\end{table}'''
        )
