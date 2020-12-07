package io.meshtech.koasta.activity

import android.content.Intent
import android.net.Uri
import android.os.Build
import android.os.Bundle
import android.text.Html
import android.text.SpannableStringBuilder
import android.text.method.LinkMovementMethod
import android.text.style.ClickableSpan
import android.text.style.URLSpan
import android.view.MenuItem
import android.view.View
import android.widget.ProgressBar
import android.widget.ScrollView
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import io.meshtech.koasta.R
import io.meshtech.koasta.net.ApiLegalDocument
import io.meshtech.koasta.net.IApi
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.MainScope
import kotlinx.coroutines.launch
import org.commonmark.parser.Parser
import org.commonmark.renderer.html.HtmlRenderer
import org.koin.android.ext.android.inject
import java.lang.ref.WeakReference


class LegalActivity : AppCompatActivity(), CoroutineScope by MainScope() {
  companion object {
    const val INTENT_EXTRA_LEGAL_DOCUMENT_TYPE = "legal-document-type"
  }

  private lateinit var document: ApiLegalDocument
  private val api: IApi by inject()
  private lateinit var scrollview: ScrollView
  private lateinit var content: TextView
  private lateinit var progress: ProgressBar
  private var job: Job? = null

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    setContentView(R.layout.activity_legal)
    setSupportActionBar(findViewById(R.id.toolbar))

    scrollview = findViewById(R.id.legal_scrollview)
    content = findViewById(R.id.legal_content)
    progress = findViewById(R.id.legal_progress)

    supportActionBar?.setDisplayHomeAsUpEnabled(true)
    supportActionBar?.title = ""

    document = ApiLegalDocument.fromValue(intent.getStringExtra(INTENT_EXTRA_LEGAL_DOCUMENT_TYPE) ?: "terms-and-conditions")

    job = doFetch()
  }

  override fun onOptionsItemSelected(item: MenuItem): Boolean {
    val id: Int = item.itemId
    return if (id == android.R.id.home) {
      onBackPressed()
      true
    } else {
      super.onOptionsItemSelected(item)
    }
  }

  private fun doFetch() = launch {
    val result = api.fetchLegalDocument(document)
    if (result.data == null) {
      showError()
    } else {
      showContent(result.data)
    }
  }

  private fun showError() {
    Toast.makeText(this, getString(R.string.legal_error), Toast.LENGTH_LONG).show()
  }

  @Suppress("DEPRECATION")
  private fun showContent(content: String) {
    progress.visibility = View.GONE
    scrollview.visibility = View.VISIBLE

    val parser = Parser.builder().build()
    val document = parser.parse(content)

    val renderer = HtmlRenderer.builder().build()
    val html = renderer.render(document)

    setTextViewHTML(this.content, html)
  }

  private fun makeLinkClickable(
    strBuilder: SpannableStringBuilder,
    span: URLSpan?
  ) {
    val start = strBuilder.getSpanStart(span)
    val end = strBuilder.getSpanEnd(span)
    val flags = strBuilder.getSpanFlags(span)
    val that = WeakReference(this)
    val clickable: ClickableSpan = object : ClickableSpan() {
      override fun onClick(widget: View) {
        val there = that.get()
        if (there != null) {
          val browserIntent = Intent(Intent.ACTION_VIEW, Uri.parse(span?.url ?: ""))
          there.startActivity(browserIntent)
        }
      }
    }
    strBuilder.setSpan(clickable, start, end, flags)
    strBuilder.removeSpan(span)
  }

  private fun setTextViewHTML(text: TextView, html: String?) {
    val sequence: CharSequence

    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
      sequence = Html.fromHtml(html, Html.FROM_HTML_MODE_COMPACT)
    } else {
      sequence = Html.fromHtml(html)
    }

    val strBuilder = SpannableStringBuilder(sequence)
    val urls =
      strBuilder.getSpans(0, sequence.length, URLSpan::class.java)
    for (span in urls) {
      makeLinkClickable(strBuilder, span)
    }
    text.text = strBuilder
    text.movementMethod = LinkMovementMethod.getInstance()
  }
}
