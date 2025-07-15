namespace ElasticSearchPlayGround.Services;

// Will remove after finish the conversion
public class FacetSearchModel
{
    public long Count { get; set; }

    public Dictionary<string, Dictionary<string, decimal>>? Facets { get; set; }
}

public class ElasticSearchResponseModel<T> : FacetSearchModel
{
    public List<T> Hits { get; set; } = new();
}