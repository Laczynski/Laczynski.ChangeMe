using System.Net;

namespace ChangeMe.Backend.Infrastructure.Email;

public static class BrandedEmailTemplates
{
  private const string FontStack =
    "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif";

  private const string ColorPageBackground = "#f8fafc";
  private const string ColorCardBackground = "#ffffff";
  private const string ColorCardBorder = "#e2e8f0";
  private const string ColorHeading = "#0f172a";
  private const string ColorBody = "#334155";
  private const string ColorMuted = "#64748b";
  private const string ColorPrimary = "#10b981";
  private const string ColorPrimaryContrast = "#ffffff";
  private const string ColorLink = "#10b981";

  public static string BuildActionEmail(
    string headline,
    string summary,
    string detail,
    string actionUrl,
    string actionLabel) =>
    BuildLayout(
      preheader: summary,
      headline,
      $"""
      <p style="{ParagraphStyle}">{Encode(summary)}</p>
      <p style="{ParagraphStyle}">{Encode(detail)}</p>
      """,
      actionUrl,
      actionLabel);

  public static string BuildNotificationEmail(
    string headline,
    string message,
    string actionUrl,
    string actionLabel = "Open issue") =>
    BuildLayout(
      preheader: message,
      headline,
      $"""<p style="{ParagraphStyle}">{Encode(message)}</p>""",
      actionUrl,
      actionLabel);

  private static string BuildLayout(
    string preheader,
    string headline,
    string bodyHtml,
    string actionUrl,
    string actionLabel)
  {
    var safeUrl = Encode(actionUrl);
    var safeHeadline = Encode(headline);
    var safeActionLabel = Encode(actionLabel);
    var safePreheader = Encode(preheader);

    return $"""
      <!DOCTYPE html>
      <html lang="en">
      <head>
        <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <meta name="color-scheme" content="light" />
        <meta name="supported-color-schemes" content="light" />
        <title>{safeHeadline}</title>
      </head>
      <body style="margin:0;padding:0;background-color:{ColorPageBackground};font-family:{FontStack};">
        <div style="display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;">
          {safePreheader}
        </div>
        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:{ColorPageBackground};padding:32px 16px;">
          <tr>
            <td align="center">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:560px;background-color:{ColorCardBackground};border:1px solid {ColorCardBorder};border-radius:16px;overflow:hidden;">
                <tr>
                  <td style="padding:28px 32px 0 32px;">
                    <p style="margin:0;font-size:20px;font-weight:600;letter-spacing:-0.02em;color:{ColorPrimary};">
                      Template
                    </p>
                  </td>
                </tr>
                <tr>
                  <td style="padding:20px 32px 8px 32px;">
                    <h1 style="margin:0;font-size:24px;font-weight:600;letter-spacing:-0.02em;line-height:1.3;color:{ColorHeading};">
                      {safeHeadline}
                    </h1>
                  </td>
                </tr>
                <tr>
                  <td style="padding:8px 32px 24px 32px;font-size:15px;line-height:1.6;color:{ColorBody};">
                    {bodyHtml}
                    <table role="presentation" cellpadding="0" cellspacing="0" style="margin:28px 0 8px 0;">
                      <tr>
                        <td style="border-radius:8px;background-color:{ColorPrimary};">
                          <a href="{safeUrl}" style="display:inline-block;padding:12px 24px;font-size:15px;font-weight:600;color:{ColorPrimaryContrast};text-decoration:none;">
                            {safeActionLabel}
                          </a>
                        </td>
                      </tr>
                    </table>
                    <p style="margin:24px 0 0 0;font-size:13px;line-height:1.5;color:{ColorMuted};">
                      Or copy this link into your browser:
                    </p>
                    <p style="margin:8px 0 0 0;font-size:13px;line-height:1.5;word-break:break-all;">
                      <a href="{safeUrl}" style="color:{ColorLink};text-decoration:underline;">{safeUrl}</a>
                    </p>
                  </td>
                </tr>
                <tr>
                  <td style="padding:20px 32px 28px 32px;border-top:1px solid {ColorCardBorder};font-size:12px;line-height:1.5;color:{ColorMuted};">
                    This message was sent by ChangeMe. If you did not expect it, you can safely ignore this email.
                  </td>
                </tr>
              </table>
            </td>
          </tr>
        </table>
      </body>
      </html>
      """;
  }

  private const string ParagraphStyle =
    "margin:0 0 16px 0;font-size:15px;line-height:1.6;color:#334155;";

  private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
