// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.TimeSeriesInsights.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// The interpolation operation to be performed on the raw data points.
    /// Currently, only sampling of interpolated time series is allowed.
    /// Allowed aggregate function - eg: left($value). Can be null if no
    /// interpolation needs to be applied.
    /// </summary>
    public partial class Interpolation
    {
        /// <summary>
        /// Initializes a new instance of the Interpolation class.
        /// </summary>
        public Interpolation()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the Interpolation class.
        /// </summary>
        /// <param name="kind">The type of interpolation technique : "Linear"
        /// or "Step". Possible values include: 'Linear', 'Step'</param>
        /// <param name="boundary">The time range to the left and right of the
        /// search span to be used for Interpolation. This is helpful in
        /// scenarios where the data points are missing close to the start or
        /// end of the input search span. Can be null.</param>
        public Interpolation(string kind = default(string), InterpolationBoundary boundary = default(InterpolationBoundary))
        {
            Kind = kind;
            Boundary = boundary;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the type of interpolation technique : "Linear" or
        /// "Step". Possible values include: 'Linear', 'Step'
        /// </summary>
        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        /// <summary>
        /// Gets or sets the time range to the left and right of the search
        /// span to be used for Interpolation. This is helpful in scenarios
        /// where the data points are missing close to the start or end of the
        /// input search span. Can be null.
        /// </summary>
        [JsonProperty(PropertyName = "boundary")]
        public InterpolationBoundary Boundary { get; set; }

    }
}
