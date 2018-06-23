using System;
using System.Collections.Generic;

class BTNode
{
    public BTArea area;
    public BTNode parent;
    public BTNode child1 = null, child2 = null;

    public BTNode(BTArea a, BTNode p)
    {
        area = a;
        parent = p;
    }

    public BTNode(BTArea a)
    {
        area = a;
        parent = null;
    }
}