using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Class to separate methods into multiple frames. Only one 'procedure' may be executed in a single frame.
/// This can be usefull when multiple simulations are run at once, where we want intermittent output.
/// </summary>
public class FrameExecuter
{
    List<Action> procedures = new List<Action>();
    private int index = 0;

    /// <summary>
    /// Adds a method to the list of methods that have to be executed.
    /// Be aware for parameters of your method using value types and reference types, a new object might have to be made for reference types.
    /// </summary>
    /// <param name="act"> () => {MethodToBeExectured();} </param>
    public void Add(Action act)
    {
        procedures.Add(act);
    }

    /// <summary>
    /// Execute only the current 'procedure'.
    /// </summary>
    public void Update()
    {
        if(index < procedures.Count)
        {
            procedures[index].Invoke();
            index++;
        }
    }
}
