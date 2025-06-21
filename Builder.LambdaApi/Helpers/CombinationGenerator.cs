namespace Builder.LambdaApi.Helpers;


/*
This is an incredibly janky and resource consuming operation to run in order to get the valid sets of items we want.
To optimize, we either need to in the future:
1. Create a new many-to-many relationship with champions to items, and then run a hash on that
2. Do a lot of prevalidation to make sure we dont pass stupid combinations into the source list that were not going to be considered.
*/
public static class CombiationGenerator
{
    public static List<List<int>> GetSubsets(int listLength, int sublistLength)
    {
        List<List<int>> subsets = new List<List<int>>();
        subsets.Add(new List<int>());

        for (int i = 0; i < listLength; i++)
        {
            int n = subsets.Count;
            for (int j = 0; j < n; j++)
            {
                List<int> newSubset = new List<int>(subsets[j]);
                newSubset.Add(i);
                subsets.Add(newSubset);
            }
        }
        return subsets.Where(subset => subset.Count == sublistLength).ToList();
    }
}