using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.SEO;

/// <summary>
/// Detects the CMS / site-builder platform, JS/CSS frameworks, and reCAPTCHA
/// used by the page and exposes them as informational SEO audits.
/// </summary>
internal static class TechRules
{
    // -------------------------------------------------------------------------
    // Platform / CMS rules
    // Each entry: Strong signals (any match → detected) + Weak signals (for
    // confirmation when no strong match is found).
    // -------------------------------------------------------------------------
    private static readonly Dictionary<string, (string[] Strong, string[] Weak)> PlatformRules = new()
    {
        ["WordPress"] = (
            ["wp-content/", "wp-includes/", "/wp-json/"],
            ["wp-emoji", "generator\" content=\"WordPress"]),

        ["Wix"] = (
            ["static.wixstatic.com", "wixCIAssets", "wix-code"],
            ["wixui-", "Wix.com Website Builder"]),

        ["Shopify"] = (
            ["cdn.shopify.com", "Shopify.theme"],
            ["myshopify.com", "shopify-section"]),

        ["Squarespace"] = (
            ["static1.squarespace.com", "squarespace-cdn.com"],
            ["squarespace.com"]),

        ["Webflow"] = (
            ["webflow.js", "w-webflow-badge"],
            ["data-wf-site", "data-wf-page"]),

        ["Joomla"] = (
            ["/media/jui/", "Joomla! -"],
            ["com_content", "joomla"]),

        ["Drupal"] = (
            ["/sites/default/files/", "Drupal.settings"],
            ["drupal.js", "generator\" content=\"Drupal"]),

        ["Umbraco"] = (
            ["umbraco/"],
            ["X-Umbraco-Version"]),

        ["TYPO3"] = (
            ["typo3conf/", "typo3temp/"],
            []),

        ["Blogger"] = (
            ["blogspot.com"],
            ["blogger.com/static"]),

        ["Weebly"] = (
            ["cdn2.editmysite.com"],
            ["weebly.com"]),

        ["Jimdo"] = (
            ["jimdo.com", "jimdofree.com"],
            []),

        ["GoDaddy Builder"] = (
            ["godaddysites.com"],
            []),

        ["Hostinger"] = (
            ["hostingersite.com"],
            ["hostinger-builder"]),

        ["Duda"] = (
            ["irp.cdn-website.com"],
            ["duda.co"]),

        ["NuvemShop"] = (
            ["nuvemshop.com.br", "tiendanube.com"],
            []),

        ["LojaIntegrada"] = (
            ["lojaintegrada.com.br"],
            []),

        ["Tray"] = (
            ["tray.com.br"],
            ["traycloud"]),

        ["Hotmart"] = (
            ["hotmart.com/checkout"],
            ["sck.hotmart"]),

        ["Kiwify"] = (
            ["kiwify.com.br"],
            []),

        ["Cartpanda"] = (
            ["cartpanda.com"],
            []),
    };

    // -------------------------------------------------------------------------
    // Cookie-based platform detection (last-resort fallback only)
    // Only applied when no HTML/script signal was found for that platform.
    // Uses prefix matching so e.g. "wordpress_logged_in_" also matches
    // "wordpress_logged_in_abc123hash".
    // -------------------------------------------------------------------------
    private static readonly Dictionary<string, string[]> PlatformCookieSignals = new()
    {
        ["WordPress"]   = ["wordpress_logged_in_", "wordpress_sec_", "wp-settings-", "wp_lang"],
        ["Umbraco"]     = ["UMB_UCONTEXT", "UMB_Kaede", "UMB_SESSION", "UMB-XSRF-TOKEN", "UMB-XSRF-V"],
        ["Shopify"]     = ["_shopify_y", "_shopify_s", "_shopify_fs", "cart_sig", "shopify_pay_redirect"],
        ["Joomla"]      = ["joomla_user_state", "joomla_remember_me_"],
        ["Drupal"]      = ["SSESS", "Drupal.visitor."],
        ["TYPO3"]       = ["be_typo_user", "fe_typo_user"],
        ["Squarespace"] = ["ss_cid", "ss_cvr", "ss_cpvisit", "squarespace-anonymous-user"],
        ["Wix"]         = ["svSession", "ssr-caching"],
        ["Magento"]     = ["mage-cache-sessid", "mage-messages", "mage-cache-storage"],
        ["OpenCart"]    = ["OCSESSID"],
    };

    // -------------------------------------------------------------------------
    // JS framework / CSS library rules
    // Signals are matched against script src attributes and inline script text.
    // -------------------------------------------------------------------------
    private static readonly Dictionary<string, (string[] Strong, string[] Weak)> FrameworkRules = new()
    {
        ["React"] = (
            ["react.production.min.js", "react.development.js", "react-dom"],
            ["__REACT_DEVTOOLS_GLOBAL_HOOK__", "ReactDOM.render", "ReactDOM.createRoot", "createElement(React"]),

        ["Next.js"] = (
            ["/_next/static/", "__NEXT_DATA__"],
            ["next/dist/", "__next"]),

        ["Vue.js"] = (
            ["vue.min.js", "vue.esm.js", "vue@", "/vue/dist/vue"],
            ["new Vue(", "createApp(", "__vue_app__"]),

        ["Nuxt.js"] = (
            ["/_nuxt/", "__nuxt"],
            ["nuxt.js", "nuxtjs.org"]),

        ["Angular"] = (
            ["angular.min.js", "angular/bundles/"],
            ["ng-version=", "ng-app=", "platformBrowserDynamic"]),

        ["Svelte"] = (
            ["svelte/internal", "__svelte"],
            ["svelte-", ".svelte"]),

        ["Ember.js"] = (
            ["ember.min.js", "ember.prod.js", "emberjs.com"],
            ["Ember.Application", "App = Ember"]),

        ["Backbone.js"] = (
            ["backbone-min.js", "backbone.js"],
            ["Backbone.View", "Backbone.Model"]),

        ["Alpine.js"] = (
            ["alpinejs", "alpine.js", "alpine.min.js"],
            ["x-data=", "x-bind:", "x-on:"]),

        ["HTMX"] = (
            ["htmx.min.js", "htmx.js", "unpkg.com/htmx"],
            ["hx-get=", "hx-post=", "hx-swap="]),

        ["jQuery"] = (
            ["jquery.min.js", "jquery.js", "jquery-", "code.jquery.com", "ajax.googleapis.com/ajax/libs/jquery"],
            ["jQuery(", "$(document).ready"]),

        ["Bootstrap"] = (
            ["bootstrap.min.js", "bootstrap.min.css", "bootstrap.bundle", "cdn.jsdelivr.net/npm/bootstrap",
             "stackpath.bootstrapcdn.com", "maxcdn.bootstrapcdn.com"],
            ["class=\"container\"", "class=\"row\"", "class=\"col-"]),

        ["Tailwind CSS"] = (
            ["tailwind.min.css", "cdn.tailwindcss.com", "tailwindcss"],
            ["class=\"flex ", "class=\"grid ", "class=\"text-", "class=\"bg-"]),

        ["Bulma"] = (
            ["bulma.min.css", "bulma.css", "bulma.io"],
            ["class=\"hero\"", "class=\"navbar\""]),

        ["Foundation"] = (
            ["foundation.min.css", "foundation.min.js"],
            ["class=\"callout\"", "zurb"]),

        ["Materialize CSS"] = (
            ["materialize.min.css", "materialize.min.js"],
            ["class=\"materialize\"", "waves-effect"]),

        ["Semantic UI"] = (
            ["semantic.min.css", "semantic.min.js", "semantic-ui"],
            ["class=\"ui container\"", "class=\"ui button\""]),

        ["Stimulus"] = (
            ["stimulus.js", "@hotwired/stimulus"],
            ["data-controller=", "data-action=", "data-target="]),

        ["Lit"] = (
            ["lit.min.js", "lit-element", "@lit/reactive-element"],
            ["LitElement", "customElement"]),
    };

    // -------------------------------------------------------------------------
    // reCAPTCHA signals
    // -------------------------------------------------------------------------
    private static readonly string[] RecaptchaStrong =
    [
        "google.com/recaptcha",
        "gstatic.com/recaptcha",
        "recaptcha/api.js",
        "recaptcha/enterprise.js",
    ];

    private static readonly string[] RecaptchaWeak =
    [
        "g-recaptcha",
        "grecaptcha",
        "data-sitekey=",
    ];

    // -------------------------------------------------------------------------
    // Public entry points
    // -------------------------------------------------------------------------

    /// <summary>
    /// Runs all technology-detection checks and returns one audit per group:
    /// detected platform, detected frameworks, and reCAPTCHA presence.
    /// </summary>
    /// <param name="doc">Parsed HTML document.</param>
    /// <param name="scripts">Script elements from the page.</param>
    /// <param name="cookies">
    /// Optional raw cookie string (format: "name=value; name2=value2").
    /// Cookie names are used as a last-resort signal when no HTML or script
    /// signals are found for a given platform.
    /// </param>
    public static List<SeoAudit> Execute(IDocument doc, List<IHtmlScriptElement> scripts, string? cookies = null)
    {
        var rawHtml = doc.DocumentElement?.OuterHtml ?? string.Empty;
        var corpus = BuildCorpus(rawHtml, scripts);

        var platforms = DetectPlatforms(rawHtml, cookies);
        var (js, css) = DetectFrameworks(corpus);
        var recaptcha = DetectRecaptcha(corpus);

        var audits = new List<SeoAudit>
        {
            new()
            {
                Title = "CMS / Site Builder",
                Status = AuditStatus.Info,
                Value = platforms.Count > 0
                    ? string.Join(", ", platforms)
                    : "No known CMS or site builder detected."
            },
            new()
            {
                Title = "JavaScript Frameworks",
                Status = AuditStatus.Info,
                Value = js.Count > 0
                    ? string.Join(", ", js)
                    : "No known JS framework detected."
            },
            new()
            {
                Title = "CSS Frameworks / Libraries",
                Status = AuditStatus.Info,
                Value = css.Count > 0
                    ? string.Join(", ", css)
                    : "No known CSS framework detected."
            },
            new()
            {
                Title = "reCAPTCHA",
                Status = AuditStatus.Info,
                Value = recaptcha ? "reCAPTCHA detected." : "reCAPTCHA not detected.",
                Recommendation = recaptcha
                    ? null
                    : "If your site has forms or login pages, consider adding reCAPTCHA to protect against bots and spam."
            }
        };

        return audits;
    }

    /// <summary>
    /// Detects the technology stack of the page and returns a strongly-typed
    /// <see cref="TechResult"/> without producing any <see cref="SeoAudit"/> entries.
    /// Use this when you only need the tech stack without running a full audit.
    /// </summary>
    /// <param name="doc">Parsed HTML document.</param>
    /// <param name="scripts">Script elements from the page.</param>
    /// <param name="cookies">
    /// Optional raw cookie string (format: "name=value; name2=value2").
    /// Cookie names are used as a last-resort signal when no HTML or script
    /// signals are found for a given platform.
    /// </param>
    public static TechResult Detect(IDocument doc, List<IHtmlScriptElement> scripts, string? cookies = null)
    {
        var rawHtml = doc.DocumentElement?.OuterHtml ?? string.Empty;
        var corpus = BuildCorpus(rawHtml, scripts);

        var platforms = DetectPlatforms(rawHtml, cookies);
        var (js, css) = DetectFrameworks(corpus);
        var recaptcha = DetectRecaptcha(corpus);

        return new TechResult
        {
            Platforms = platforms,
            JsFrameworks = js,
            CssFrameworks = css,
            HasRecaptcha = recaptcha
        };
    }

    // -------------------------------------------------------------------------
    // Core detection helpers (shared by Execute and Detect)
    // -------------------------------------------------------------------------

    private static List<string> DetectPlatforms(string rawHtml, string? cookieString = null)
    {
        var detected = new List<string>();
        var cookies = CookieHelper.Parse(cookieString);

        foreach (var (name, (strong, weak)) in PlatformRules)
        {
            if (HasSignal(rawHtml, strong) || HasSignal(rawHtml, weak))
            {
                detected.Add(name);
                continue;
            }

            // Last resort: check cookie names when no HTML/script signal was found
            if (cookies.Count > 0 &&
                PlatformCookieSignals.TryGetValue(name, out var cookieSignals) &&
                CookieHelper.HasCookieSignal(cookies, cookieSignals))
            {
                detected.Add($"{name} (via cookies)");
            }
        }

        return detected;
    }

    private static (List<string> Js, List<string> Css) DetectFrameworks(string corpus)
    {
        var js = new List<string>();
        var css = new List<string>();

        var cssFrameworks = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Bootstrap", "Tailwind CSS", "Bulma", "Foundation",
            "Materialize CSS", "Semantic UI",
        };

        foreach (var (name, (strong, weak)) in FrameworkRules)
        {
            if (!HasSignal(corpus, strong) && !HasSignal(corpus, weak))
                continue;

            if (cssFrameworks.Contains(name))
                css.Add(name);
            else
                js.Add(name);
        }

        return (js, css);
    }

    private static bool DetectRecaptcha(string corpus) =>
        HasSignal(corpus, RecaptchaStrong) || HasSignal(corpus, RecaptchaWeak);

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds a single searchable corpus from the raw HTML plus script src/body text.
    /// </summary>
    private static string BuildCorpus(string rawHtml, List<IHtmlScriptElement> scripts)
    {
        var parts = new List<string>(scripts.Count * 2 + 1) { rawHtml };

        foreach (var s in scripts)
        {
            if (!string.IsNullOrWhiteSpace(s.Source))
                parts.Add(s.Source);

            if (!string.IsNullOrWhiteSpace(s.Text))
                parts.Add(s.Text);
        }

        return string.Concat(parts);
    }

    /// <summary>
    /// Returns <see langword="true"/> if any signal string appears in the corpus
    /// (case-insensitive).
    /// </summary>
    private static bool HasSignal(string corpus, string[] signals) =>
        signals.Any(sig => corpus.Contains(sig, StringComparison.OrdinalIgnoreCase));
}
