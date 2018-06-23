public enum NeighbourhoodType
{
    Moore, Neuman
}



class CARuleset
{
    public NeighbourhoodType neighbourhoodType;
    public int survMin, survMax, newMin, newMax;
    public CARuleset(NeighbourhoodType nt, int survMin, int survMax, int newMin, int newMax)
    {
        this.neighbourhoodType = nt;
        this.survMin = survMin;
        this.survMax = survMax;
        this.newMin = newMin;
        this.newMax = newMax;
    }

}

