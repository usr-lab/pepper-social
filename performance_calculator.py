'''
This script was used to calculate/generate the performance table.

To use it follow steps 1-3, then run the script.
'''



'''
#
#   Step 1: Enter your results
#
'''
results = {
            'Random'                        : 0.0,
            'Vector + LSTM'                 : 5.0,
            'Vector + FF'                   : 1.0,
            'CameraOnly + our + FF'         : 2.0,
            'CameraOnly + our + LSTM'       : 3.0,
            'CameraOnly + conv + FF'        : 4.0,
            'CameraOnly + conv + LSTM'      : 5.0,
            'CameraSpeed + our + FF'        : 6.0,
            'CameraSpeed + our + LSTM'      : -1.0,
            'CameraSpeed + conv + FF'       : 1.5,
            'CameraSpeed + conv + LSTM'     : 0.0,
           }

'''
#
#   Step 2: tell if you want latex-format, and if you want something printed between the entries.
#
'''
print_latex_table = True
latex_line_separator = '' #use '\\hline' to have a line between all entries

'''
#
#   Step 3: Decide lower and an upper baseline.
#
'''
lower = results['Random']
upper = results['Vector + LSTM']



#####
##### Just code below . . .
#####

if not print_latex_table:
    for name in sorted(results):
        value = results[name]
        relative = (value - lower) / (upper - lower)
        print( "{:30}\t\t{:1.3f}\t{:3.1f}%".format(name,value, relative) )
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
        relative = (value - lower) / (upper - lower)
        if value == best_val:
            print( "{:30} & \\textbf{{{:1.3f}}} & \\textbf{{{:3.1f}}}\\% \\\\ {}".format(name,value, relative, latex_line_separator) )
        print( "{:30} & {:1.3f} & {:3.1f}\\% \\\\ {}".format(name,value, relative, latex_line_separator) )
    print(
        '''\\hline
\\end{tabular}
\\end{center}
\\caption{Results of the different configurations.}
\\label{results}
\\end{table}'''
        )
