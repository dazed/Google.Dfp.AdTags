using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;

namespace Google.Dfp
{

	/// <summary>
	/// Enables the management of google ad slots.
	/// </summary>
	public static class AdTags
	{
		private class AdUnit
		{
			public string UnitName { get; set; }
			public string Size { get; set; }
			/// <summary>
			/// Should the ad be displayed straight away?
			/// </summary>
			public bool Display { get; set; }

			/// <summary>
			/// Id of the container the ad is placed in.
			/// </summary>
			public string Id { get; set; }

			/// <summary>
			/// Size mapping for responsive ads.
			/// </summary>
			/// <remarks>https://support.google.com/dfp_sb/answer/4525701?hl=en-GB</remarks>
			public string SizeMapping { get; set; }
		}

		private static HttpContextBase Context
		{
			get
			{
				if(HttpContext.Current == null)
					throw new ApplicationException("Google.Dfp.AdTags This must be run in a web application");
				return new HttpContextWrapper(HttpContext.Current);
			}
		}

		private static List<AdUnit> AdUnits
		{
			get
			{
				var placements = Context.Items["GoogleAdUnits"] as List<AdUnit>;
				if (placements == null)
				{
					placements = new List<AdUnit>();
					Context.Items["GoogleAdUnits"] = placements;
				}
				return placements;
			}
		}

		private static Dictionary<String, IEnumerable<String>> SizeUnits
		{
			get
			{
				var placements = Context.Items["GoogleSizeUnits"] as Dictionary<String, IEnumerable<String>>;
				if (placements == null)
				{
					placements = new Dictionary<String, IEnumerable<String>>();
					Context.Items["GoogleSizeUnits"] = placements;
				}
				return placements;
			}
		}


		/// <summary>
		/// Placeholder div that is replaced with ads.
		/// </summary>
		/// <returns>Div container.</returns>
		public static IHtmlString Placeholder(string unitName, string size)
		{
			return Placeholder(unitName, size, String.Empty);
		}

		/// <summary>
		/// Placeholder div that is replaced with ads.
		/// </summary>
		/// <param name="unitName">Name of the ad unit as set in DFP.</param>
		/// <param name="size">Specify creative sizes in the googletag.defineSlot() function. To allow multiple sizes to serve to the ad slot, you can use a comma-separated list.</param>
		/// <param name="cssClass">CSS class to add to the ad unit div container.</param>
		/// <returns>Div container.</returns>
		public static IHtmlString Placeholder(string unitName, string size, string cssClass)
		{
			return Placeholder(unitName, size, cssClass, "div");
		}

		/// <summary>
		/// Placeholder that is replaced with ads.
		/// </summary>
		/// <param name="unitName">Name of the ad unit as set in DFP.</param>
		/// <param name="size">Specify creative sizes in the googletag.defineSlot() function. To allow multiple sizes to serve to the ad slot, you can use a comma-separated list.</param>
		/// <param name="cssClass">CSS class to add to the ad unit div container.</param>
		/// <param name="tagName">Type of parent container, must be block</param>
		/// <returns>HTML container object.</returns>
		public static IHtmlString Placeholder(string unitName, string size, string cssClass, string tagName)
		{
			return Placeholder(unitName, size, cssClass, tagName, null);
		}

		/// <summary>
		/// Placeholder that is replaced with ads.
		/// </summary>
		/// <param name="unitName">Name of the ad unit as set in DFP.</param>
		/// <param name="size">Specify creative sizes in the googletag.defineSlot() function. To allow multiple sizes to serve to the ad slot, you can use a comma-separated list.</param>
		/// <param name="cssClass">CSS class to add to the ad unit div container.</param>
		/// <param name="tagName">Type of parent container, must be block</param>
		/// <param name="sizeMapping">Specify creative sizes in the googletag.defineSlot() function. To allow multiple sizes to serve to the ad slot, you can use a comma-separated list.</param>
		/// <returns>HTML container object.</returns>
		public static IHtmlString Placeholder(string unitName, string size, string cssClass, string tagName, string sizeMapping)
		{
			return Placeholder(unitName, size, cssClass, tagName, sizeMapping, true);
		}


		private static int _adCounter;

		/// <summary>
		/// Placeholder that is replaced with ads.
		/// </summary>
		/// <param name="unitName">Name of the ad unit as set in DFP.</param>
		/// <param name="size">Specify creative sizes in the googletag.defineSlot() function. To allow multiple sizes to serve to the ad slot, you can use a comma-separated list.</param>
		/// <param name="cssClass">CSS class to add to the ad unit div container.</param>
		/// <param name="tagName">Type of parent container, must be block</param>
		/// <param name="sizeMapping">Specify creative sizes in the googletag.defineSlot() function. To allow multiple sizes to serve to the ad slot, you can use a comma-separated list.</param>
		/// <param name="display">Should display be called when the dfp script is initialised?</param>
		/// <returns>HTML container object.</returns>
		public static IHtmlString Placeholder(string unitName, string size, string cssClass, string tagName, string sizeMapping, bool display)
		{
			if (String.IsNullOrWhiteSpace(unitName)) throw new ArgumentNullException("unitName");
			if (String.IsNullOrWhiteSpace(tagName)) throw new ArgumentNullException("tagName");

			var containerId = "div-gpt-ad-" + _adCounter;

			string placeholder = String.Format(
				CultureInfo.InvariantCulture,
				"<{0} id=\"{1}\" class=\"{2}\" data-cb-ad-id=\"{3}\"><!-- {3} --></{0}>",
				tagName,
				containerId,
				cssClass,
				unitName);

			var unit = new AdUnit
			{
				UnitName = unitName,
				Size = size,
				Display = display,
				Id = containerId
			};

			if (sizeMapping != null)
			{
				// Confirm the mapping exists
				if (!SizeUnits.ContainsKey(sizeMapping))
					throw new ArgumentException(String.Format("Size unit '{0}' not defined. Sizes must be defined before adding to a unit.", sizeMapping), "sizeMapping");
				unit.SizeMapping = sizeMapping;
			}

			AdUnits.Add(unit);


			_adCounter++;

			return new HtmlString(placeholder);
		}

		/// <summary>
		/// Defines an ad unit in publisher tags without creating the container on the page.
		/// </summary>
		/// <param name="unitName">Name of the ad unit as set in DFP.</param>
		/// <param name="size">Ad unit size.</param>
		/// <param name="id">ID of the ad unit container.</param>
		public static void DefineAdUnit(string unitName, string size, string id)
		{
			if (String.IsNullOrWhiteSpace(unitName)) throw new ArgumentNullException("unitName");
			if (String.IsNullOrWhiteSpace(id)) throw new ArgumentNullException("id");

			var unit = new AdUnit
			{
				UnitName = unitName,
				Size = size,
				Display = false,
				Id = id
			};


			AdUnits.Add(unit);
		}


		/// <summary>
		/// Adds a size unit for serving responsive ads.
		/// https://support.google.com/dfp_premium/answer/3423562?hl=en
		/// </summary>
		/// <param name="name">Unique size mapping name.</param>
		/// <param name="sizes">an array of mappings in the following form: [ [ [ 1024, 768 ], [ [ 970, 250 ] ] ], [ [ 980, 600 ], [ [ 728, 90 ], [ 640, 480 ] ] ], ...], (browser size, slot sizes), which should be ordered from highest to lowest priority.</param>
		public static void AddSizeMapping(string name, IEnumerable<string> sizes)
		{
			if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
			if (SizeUnits.ContainsKey(name)) throw new ArgumentException(String.Format("Size mapping '{0}' already defined.", name), "name");
			if (sizes == null) throw new ArgumentNullException("sizes");

			SizeUnits.Add(name, sizes);
		}

		private static String WriteMappingName(string name)
		{
			return name + "GoogleAdMapping";
		}

		/// <summary>
		/// Script that must be located in the document head.
		/// </summary>
		/// <param name="networkCode">Google ad sense network code.</param>
		/// <returns>Script tags.</returns>
		public static IHtmlString FooterTag(string networkCode)
		{
			return FooterTag(networkCode, null);
		}

		/// <summary>
		/// Script that must be located in the document head.
		/// </summary>
		/// <param name="networkCode">Google ad sense network code.</param>
		/// <param name="targeting">Key-values to target ads.</param>
		/// <returns>Script tags.</returns>
		public static IHtmlString FooterTag(string networkCode, Dictionary<string, string> targeting)
		{
			var sb = new StringBuilder();

			if (String.IsNullOrWhiteSpace(networkCode)) //Just hide all the divs
			{
				sb.AppendLine("<script>");
				foreach(var ad in AdUnits)
				{
					sb.AppendFormat(
						"document.getElementById('{0}').style.display='none';{1}",
						ad.Id,
						Environment.NewLine);
				}
				sb.AppendLine("</script>");
			}
			else
			{
				// General script
				sb.AppendLine(@"<script>
window.DG={};
DG.ads={};
var googletag = googletag || {};
googletag.cmd = googletag.cmd || [];
(function() {
var gads = document.createElement('script');
gads.async = true;
gads.type = 'text/javascript';
var useSSL = 'https:' == document.location.protocol;
gads.src = (useSSL ? 'https:' : 'http:') + 
'//www.googletagservices.com/tag/js/gpt.js';
var node = document.getElementsByTagName('script')[0];
node.parentNode.insertBefore(gads, node);
})();");

				sb.AppendLine(@"googletag.cmd.push(function(){");
				// Define the sizes
				foreach (var size in SizeUnits)
				{
					sb.AppendFormat("var {0} = googletag.sizeMapping().{1}", WriteMappingName(size.Key), Environment.NewLine);
					foreach (var variation in size.Value)
					{
						sb.AppendFormat("addSize({0}).{1}", variation, Environment.NewLine);
					}
					sb.AppendLine("build();");
				}

				// Define the slots
				foreach (var ad in AdUnits)
				{
					var sizeMappingFunction = String.Empty;
					if (ad.SizeMapping != null)
					{
						sizeMappingFunction = String.Format(".defineSizeMapping({0})", WriteMappingName(ad.SizeMapping));
					}
					sb.AppendFormat(
						CultureInfo.InvariantCulture,
						"DG.ads['{1}']=googletag.defineSlot('/{0}/{1}', {2}, '{3}'){4}.addService(googletag.pubads());{5}",
						networkCode,
						ad.UnitName,
						ad.Size,
						ad.Id,
						sizeMappingFunction,
						Environment.NewLine);
				}
				if (targeting != null)
				{
					foreach (var target in targeting)
					{
						sb.AppendFormat("googletag.pubads().setTargeting('{0}','{1}');{2}", target.Key, target.Value, Environment.NewLine);
					}
				}
				sb.Append(@"googletag.pubads().enableSingleRequest();
googletag.pubads().collapseEmptyDivs();
googletag.enableServices();
});");
				DisplayAds(sb);
				sb.AppendLine("</script>");
			}

			return new HtmlString(sb.ToString());
		}

		private static void DisplayAds(StringBuilder sb)
		{
			sb.AppendLine();
			// Finaly push the adds
			foreach (var ad in AdUnits)
			{
				if (ad.Display)
				{
					sb.AppendFormat("googletag.cmd.push(function() {{ googletag.display('{0}'); }});{1}", ad.Id, Environment.NewLine);
				}
			}
		}
	}
}
