package io.meshtech.koasta.control;

import java.util.Random;

import android.content.Context;
import android.util.AttributeSet;
import android.view.KeyEvent;
import android.view.inputmethod.EditorInfo;
import android.view.inputmethod.InputConnection;
import android.view.inputmethod.InputConnectionWrapper;

public class ZanyEditText extends androidx.appcompat.widget.AppCompatEditText {

  public interface ZanyEditTextBackspaceListener {
    void onZanyBackspace(ZanyEditText field);
  }

  private Random r = new Random();
  private ZanyEditTextBackspaceListener listener = null;

  public ZanyEditTextBackspaceListener getBackspaceListener() {
    return listener;
  }

  public void setBackspaceListener(ZanyEditTextBackspaceListener listener) {
    this.listener = listener;
  }

  public ZanyEditText(Context context, AttributeSet attrs, int defStyle) {
    super(context, attrs, defStyle);
  }

  public ZanyEditText(Context context, AttributeSet attrs) {
    super(context, attrs);
  }

  public ZanyEditText(Context context) {
    super(context);
  }

  private void triggerListener() {
    if (this.listener != null) {
      this.listener.onZanyBackspace(this);
    }
  }

  @Override
  public InputConnection onCreateInputConnection(EditorInfo outAttrs) {
    return new ZanyInputConnection(super.onCreateInputConnection(outAttrs),
      true);
  }

  private class ZanyInputConnection extends InputConnectionWrapper {

    public ZanyInputConnection(InputConnection target, boolean mutable) {
      super(target, mutable);
    }

    @Override
    public boolean sendKeyEvent(KeyEvent event) {
      if (event.getAction() == KeyEvent.ACTION_DOWN
        && event.getKeyCode() == KeyEvent.KEYCODE_DEL) {
        ZanyEditText.this.triggerListener();
      }
      return super.sendKeyEvent(event);
    }


    @Override
    public boolean deleteSurroundingText(int beforeLength, int afterLength) {
      // magic: in latest Android, deleteSurroundingText(1, 0) will be called for backspace
      if (beforeLength == 1 && afterLength == 0) {
        // backspace
        return sendKeyEvent(new KeyEvent(KeyEvent.ACTION_DOWN, KeyEvent.KEYCODE_DEL))
          && sendKeyEvent(new KeyEvent(KeyEvent.ACTION_UP, KeyEvent.KEYCODE_DEL));
      }

      return super.deleteSurroundingText(beforeLength, afterLength);
    }

  }

}
