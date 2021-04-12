using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FrameExecuter
{
    List<Action> procedures = new List<Action>();
    private int index = 0;

    public void Add(Action act)
    {
        procedures.Add(act);
    }

    public void Update()
    {
        if(index < procedures.Count)
        {
            procedures[index].Invoke();
            index++;
        }
    }
}
