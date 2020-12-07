package io.meshtech.koasta.fragment

import android.content.Intent
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.fragment.app.Fragment
import io.meshtech.koasta.R
import io.meshtech.koasta.activity.LegalActivity
import io.meshtech.koasta.activity.OnboardingActivity
import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.net.ApiLegalDocument
import org.koin.android.ext.android.inject

class SettingsFragment : Fragment() {
  private val session: ISessionManager by inject()
  private lateinit var signInButton: ViewGroup
  private lateinit var shareButton: ViewGroup
  private lateinit var contactButton: ViewGroup
  private lateinit var legalButton: ViewGroup
  private lateinit var accountDetails: TextView
  private lateinit var signOutButton: ViewGroup

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    arguments?.let {
    }
  }

  override fun onCreateView(
    inflater: LayoutInflater, container: ViewGroup?,
    savedInstanceState: Bundle?
  ): View? {
    return inflater.inflate(R.layout.fragment_settings, container, false)
  }

  override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
    super.onViewCreated(view, savedInstanceState)

    signInButton = view.findViewById(R.id.settings_button_sign_in)
    shareButton = view.findViewById(R.id.settings_button_share)
    contactButton = view.findViewById(R.id.settings_button_contact)
    legalButton = view.findViewById(R.id.settings_button_legal)
    accountDetails = view.findViewById(R.id.settings_account_details)
    signOutButton = view.findViewById(R.id.settings_button_sign_out)

    shareButton.setOnClickListener { share() }
    contactButton.setOnClickListener { contact() }
    legalButton.setOnClickListener { legal() }
    signOutButton.setOnClickListener { signOut() }

    accountDetails.text = session.accountDescription
  }

  companion object {
    @JvmStatic
    fun newInstance() =
      SettingsFragment().apply {
        arguments = Bundle().apply {
        }
      }
  }

  private fun share() {
    val sendIntent: Intent = Intent().apply {
      action = Intent.ACTION_SEND
      putExtra(Intent.EXTRA_TEXT, "Make ordering easy with Koasta. https://www.koasta.com")
      type = "text/plain"
    }

    val shareIntent = Intent.createChooser(sendIntent, null)
    startActivity(shareIntent)
  }

  private fun contact() {
    val sendIntent: Intent = Intent().apply {
      action = Intent.ACTION_SEND
      type = "message/rfc822"
      putExtra(Intent.EXTRA_EMAIL, arrayOf("hello@koasta.com"))
    }

    val shareIntent = Intent.createChooser(sendIntent, null)
    startActivity(shareIntent)
  }

  private fun legal() {
    val intent = Intent(requireActivity(), LegalActivity::class.java)
    intent.putExtra(LegalActivity.INTENT_EXTRA_LEGAL_DOCUMENT_TYPE, ApiLegalDocument.TERMS_AND_CONDITIONS.value)
    requireActivity().startActivity(intent)
  }

  private fun signOut() {
    session.purge()
    val intent = Intent(requireActivity(), OnboardingActivity::class.java)
    intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_TASK_ON_HOME
    requireActivity().startActivity(intent)
  }
}
