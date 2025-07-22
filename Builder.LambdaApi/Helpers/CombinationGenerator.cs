namespace Builder.LambdaApi.Helpers;


/*
This is an incredibly janky and resource consuming operation to run
in order to get the valid sets we want.
To optimize, we either need to in the future:
1. Do a lot of prevalidation to make sure we dont pass stupid combinations into the source list that were not going to be considered.
2. Produce a hopefully better algorithm :(
*/
public static class CombinationGenerator
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

    public static List<List<T>> GenerateListCombinationsWithDistance<T>(List<T> firstList, List<T> secondList, int distance)
    {
        List<List<T>> allCombinations = new List<List<T>>();
        allCombinations.Add(new List<T>(firstList));

        for (int i = 1; i <= distance; i++)
        {
            foreach (var indicesToSubstitute in GetSubsets(firstList.Count, i))
            {
                foreach (var indicesToUse in GetPermutationsWithRepetition<T>(secondList, i))
                {
                    List<T> currentCombination = new List<T>(firstList);

                    for (int k = 0; k < i; k++)
                    {
                        int indexInListA = indicesToSubstitute[k];
                        T elementFromListB = indicesToUse[k];

                        currentCombination[indexInListA] = elementFromListB;
                    }
                    
                    allCombinations.Add(currentCombination);
                }
            }
        }
        return allCombinations;
    }

    private static IEnumerable<List<T>> GetPermutationsWithRepetition<T>(List<T> sourceList, int count)
    {
        if (count == 0)
        {
            yield return new List<T>();
            yield break;
        }

        if (sourceList == null || !sourceList.Any())
        {
            yield break;
        }

        foreach (var element in sourceList)
        {
            foreach (var subPermutation in GetPermutationsWithRepetition(sourceList, count - 1))
            {
                List<T> currentPermutation = new List<T> { element };
                currentPermutation.AddRange(subPermutation);
                yield return currentPermutation;
            }
        }
    }
}